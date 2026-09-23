#!/usr/bin/env python3
"""Release every mod that has changed since its last release.

    python oni-mods-common/tools/Release/release_mods.py [repo ...] [--plan] [--bump patch|minor|major]
                                                          [--bump-for repo=level ...] [--root DIR]

With no repo names, looks at every folder next to oni-mods-common that holds a mod
(src/<Mod>/mod_info.yaml). For each one:

  - the last release is the newest v* tag; a mod with no tag yet gets its first release at
    the version it already has;
  - "changed" means commits since that tag touched what ships: src/ or publish/preview.png
    (README-only changes do not trigger a release);
  - if the version in mod_info.yaml is still the released one, it is bumped (mod_info.yaml
    and the csproj <Version>), committed as "Release vX.Y.Z" and pushed; a version you
    already bumped by hand is used as is;
  - the "## Unreleased" section of CHANGELOG.md, if it has content, becomes
    "## X.Y.Z - <date>" in that same commit, and release.py uses it as the GitHub release
    notes and the Workshop change note. Write the entry before releasing: --plan says
    "no changelog entry" for a mod that would otherwise ship with notes made of commit subjects;
  - release.py then builds, zips, tags and creates the GitHub release with the zip attached.

Mods with uncommitted changes to tracked files are skipped, never stashed or committed.
--plan only prints what would happen. A mod that is on the Steam Workshop (id in
publish/workshop-id.txt, or else found by title among your published items) gets that item
updated with the same zip right after the GitHub release (see release.py); --no-workshop
turns that off.
"""
import argparse
import datetime
import glob
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from release import find_workshop_id  # noqa: E402
RELEASE = os.path.join(HERE, "release.py")
SHIPPED = ["src", "publish/preview.png"]


def git(repo, *args, check=True):
    result = subprocess.run(("git", "-C", repo) + args, text=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if check and result.returncode:
        raise RuntimeError("git %s failed in %s: %s" % (" ".join(args), repo, result.stderr.strip()))
    return result.stdout.strip()


def read_version(repo):
    path = glob.glob(os.path.join(repo, "src", "*", "mod_info.yaml"))[0]
    return path, re.search(r"^version:\s*\"?([0-9]+\.[0-9]+\.[0-9]+)", open(path, encoding="utf-8").read(), re.M).group(1)


def bumped(version, level):
    major, minor, patch = (int(v) for v in version.split("."))
    return {"major": "%d.0.0" % (major + 1), "minor": "%d.%d.0" % (major, minor + 1),
            "patch": "%d.%d.%d" % (major, minor, patch + 1)}[level]


def replace_in(path, pattern, replacement):
    text = open(path, encoding="utf-8", newline="").read()
    new, count = re.subn(pattern, replacement, text, count=1, flags=re.M)
    if count != 1:
        raise RuntimeError("could not update the version in " + path)
    open(path, "w", encoding="utf-8", newline="").write(new)


UNRELEASED = re.compile(r"^## +\[?Unreleased\]?[^\n]*\n(.*?)(?=^## |\Z)", re.M | re.S | re.I)


def unreleased_entry(repo):
    """Content under "## Unreleased" in the repo's CHANGELOG.md, or None."""
    path = os.path.join(repo, "CHANGELOG.md")
    if not os.path.exists(path):
        return None
    match = UNRELEASED.search(open(path, encoding="utf-8").read().replace("\r\n", "\n"))
    return match.group(1).strip() if match and match.group(1).strip() else None


def release_changelog(repo, version):
    """Renames "## Unreleased" to "## <version> - <today>" and stages the file. Returns False
    when there was no entry to release."""
    path = os.path.join(repo, "CHANGELOG.md")
    if not unreleased_entry(repo):
        return False
    text = open(path, encoding="utf-8", newline="").read()
    newline = "\r\n" if "\r\n" in text else "\n"
    heading = "## %s - %s" % (version, datetime.date.today().isoformat())
    text = re.sub(r"^## +\[?Unreleased\]?[^\n]*$", heading, text, count=1, flags=re.M | re.I)
    open(path, "w", encoding="utf-8", newline="").write(text.replace("\r\n", "\n").replace("\n", newline))
    git(repo, "add", "CHANGELOG.md")
    return True


def plan_for(repo, level):
    """Returns (action, detail). action is 'skip', 'first', 'bump' or 'as-is'."""
    if git(repo, "status", "--porcelain", "--untracked-files=no"):
        return "skip", "uncommitted changes to tracked files"
    git(repo, "fetch", "--quiet", "--tags", check=False)
    if git(repo, "rev-list", "--count", "HEAD..@{upstream}", check=False) not in ("", "0"):
        return "skip", "behind its remote (a stale copy?); pull first"
    _, version = read_version(repo)
    last = git(repo, "describe", "--tags", "--abbrev=0", "--match", "v[0-9]*", check=False)
    if not last:
        return "first", "no release yet; releasing v%s" % version
    changed = git(repo, "diff", "--name-only", last, "HEAD", "--", *SHIPPED)
    if not changed:
        return "skip", "nothing shipped has changed since %s" % last
    commits = git(repo, "log", "--format=  %h %s", "%s..HEAD" % last, "--", *SHIPPED)
    if not unreleased_entry(repo):
        commits += "\n  ! no changelog entry (CHANGELOG.md ## Unreleased); notes would be the commit subjects"
    if "v" + version != last:
        return "as-is", "changed since %s; version already set to %s\n%s" % (last, version, commits)
    return "bump", "changed since %s; %s -> %s (%s)\n%s" % (last, version, bumped(version, level), level, commits)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("repos", nargs="*", help="mod repo folder names (default: all)")
    ap.add_argument("--root", default=os.path.abspath(os.path.join(HERE, "..", "..", "..")), help="folder holding the mod repos")
    ap.add_argument("--plan", action="store_true", help="only print what would be released")
    ap.add_argument("--bump", choices=("patch", "minor", "major"), default="patch")
    ap.add_argument("--no-workshop", action="store_true", help="GitHub releases only")
    ap.add_argument("--bump-for", action="append", default=[], metavar="REPO=LEVEL", help="override --bump for one repo")
    a = ap.parse_args()

    levels = dict(item.split("=", 1) for item in a.bump_for)
    names = a.repos or sorted(os.path.basename(os.path.dirname(os.path.dirname(os.path.dirname(p))))
                              for p in glob.glob(os.path.join(a.root, "*", "src", "*", "mod_info.yaml")))
    failures = 0
    for name in names:
        repo = os.path.join(a.root, name)
        if not glob.glob(os.path.join(repo, "src", "*", "mod_info.yaml")):
            print("== %s: not a mod repo under %s" % (name, a.root))
            failures += 1
            continue
        level = levels.get(name, a.bump)
        action, detail = plan_for(repo, level)
        print("== %s: %s" % (name, detail))
        if action != "skip":
            workshop_id, how = (None, "off") if a.no_workshop else find_workshop_id(repo)
            workshop = {"off": "no Workshop update (--no-workshop)", "unlisted": "not on the Workshop",
                        "file": "Workshop item %s" % workshop_id,
                        "listed": "Workshop item %s (found by title; id will be recorded)" % workshop_id}.get(how, how)
            print("  + GitHub release, " + workshop)
        if action == "skip" or a.plan:
            continue
        try:
            info_path, version = read_version(repo)
            if action == "bump":
                version = bumped(version, level)
                replace_in(info_path, r"^(version:\s*\"?)[0-9]+\.[0-9]+\.[0-9]+", r"\g<1>" + version)
                for csproj in glob.glob(os.path.join(os.path.dirname(info_path), "*.csproj")):
                    replace_in(csproj, r"(<Version>)[^<]*(</Version>)", r"\g<1>" + version + r"\g<2>")
            if release_changelog(repo, version) or action == "bump":
                git(repo, "commit", "-am", "Release v" + version)
            git(repo, "push")
            subprocess.run([sys.executable, RELEASE] + (["--no-workshop"] if a.no_workshop else []), cwd=repo, check=True)
        except (RuntimeError, subprocess.CalledProcessError) as error:
            print("!! %s failed: %s" % (name, error))
            failures += 1
    sys.exit(1 if failures else 0)


if __name__ == "__main__":
    main()
