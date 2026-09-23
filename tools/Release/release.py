#!/usr/bin/env python3
"""Build a mod, package it, tag the commit and publish a GitHub release, in one step.

    python common/tools/Release/release.py [--dry-run] [--notes "text"] [--changenote "text"] [--no-workshop]

Run it from the mod repo's root. The mods compile against the game's own assemblies, which
cannot live on a CI runner, so releases are cut locally instead of by a GitHub Action.

What it does:
  1. Refuses to run unless tracked files are all committed and HEAD is pushed.
  2. Reads the version from src/<Mod>/mod_info.yaml; the tag is v<version> and must be new.
  3. Builds src/<Mod> in Release (without deploying to the local mods folder).
  4. Stages publish/content from the build (DLL, mod.yaml, mod_info.yaml, anim/, assets/,
     and publish/preview.png scaled down to at most 512 px) and zips it to publish/<Mod>.zip.
     The zip has the mod files at its root: the layout the game and the Workshop expect.
  5. Tags, pushes the tag, and creates the GitHub release with <Mod>-<version>.zip attached.
  6. If the mod is on the Steam Workshop, uploads the same zip, the preview and
     publish/workshop-description.txt to its item (legacy API, via tools/WorkshopUpload;
     Steam must be running as the item's owner). The item id comes from
     publish/workshop-id.txt; when that file is missing, the mod.yaml title is looked up
     among your published items (WorkshopUpload list) and, if found, the id is written to
     the file and committed. A mod that is not listed is simply not on the Workshop yet.
     --no-workshop skips all of this; --workshop-id overrides the lookup.

Release notes: the section of CHANGELOG.md headed "## <version>" (release_mods.py creates it
from "## Unreleased") is the GitHub release body and, with the version on top, the Workshop
change note. Without one, GitHub generates notes from the commits and the change note is the
version plus the subjects of the commits that changed what ships. --notes and --changenote
override either.

--dry-run stops after step 4, so it doubles as "just rebuild the zip".
"""
import argparse
import glob
import os
import re
import shutil
import subprocess
import sys
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
UPLOADER = os.path.join(HERE, "..", "WorkshopUpload", "bin", "Release", "net8.0", "win-x64", "WorkshopUpload.exe")


def run(*cmd, capture=False):
    print(">", " ".join(cmd))
    result = subprocess.run(cmd, check=True, text=True, stdout=subprocess.PIPE if capture else None)
    return result.stdout.strip() if capture else None


def find_workshop_id(repo="."):
    """Item id from publish/workshop-id.txt, else by mod.yaml title among the user's published
    items. Returns (id or None, how) where how is 'file', 'listed', 'unlisted' or an error text."""
    id_file = os.path.join(repo, "publish", "workshop-id.txt")
    if os.path.exists(id_file):
        return open(id_file).read().strip(), "file"
    if not os.path.exists(UPLOADER):
        return None, "WorkshopUpload is not built"
    yaml = glob.glob(os.path.join(repo, "src", "*", "mod.yaml"))
    title = re.search(r"^title:\s*\"?(.*?)\"?\s*$", open(yaml[0], encoding="utf-8").read(), re.M).group(1)
    listing = subprocess.run([os.path.abspath(UPLOADER), "list"], cwd=os.path.dirname(os.path.abspath(UPLOADER)),
                             text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    if listing.returncode:
        return None, "WorkshopUpload list failed (is Steam running?)"
    for line in listing.stdout.splitlines():
        match = re.match(r"^(\d+)\s+(.*?)\s+k_ERemoteStoragePublishedFileVisibility", line)
        if match and match.group(2).strip().lower() == title.strip().lower():
            return match.group(1), "listed"
    return None, "unlisted"


def changelog_section(version, path="CHANGELOG.md"):
    """Body of the "## <version>" section of CHANGELOG.md (markdown, stripped), or None."""
    if not os.path.exists(path):
        return None
    text = open(path, encoding="utf-8").read().replace("\r\n", "\n")
    match = re.search(r"^## \[?%s\]?\b[^\n]*\n(.*?)(?=^## |\Z)" % re.escape(version), text, re.M | re.S)
    body = match.group(1).strip() if match else ""
    return body or None


def workshop_note(version, markdown):
    """The changelog section as a Steam change note: version heading, markdown bullets kept,
    emphasis and code turned into BBCode / plain text."""
    text = re.sub(r"\*\*(.+?)\*\*", r"[b]\1[/b]", markdown)
    text = re.sub(r"`(.+?)`", r"\1", text)
    text = re.sub(r"\[([^\]]+)\]\(([^)]+)\)", r"\1 (\2)", text)
    text = re.sub(r"^### +", "", text, flags=re.M)
    return "v%s\n%s" % (version, text)


def description_is_ours(workshop_id, description, previous_tag):
    """True when it is safe to upload publish/workshop-description.txt: the LIVE description is
    still the text we uploaded last (the file as of the previous release tag). The description
    can also be edited on the Workshop page; if it was, it is saved next to the file as
    workshop-description.live.txt and left alone, to be merged by hand."""
    def normal(text):
        return "\n".join(line.rstrip() for line in text.replace("\r\n", "\n").strip().split("\n"))

    live_file = os.path.abspath(os.path.join("publish", "workshop-description.live.txt"))
    fetched = subprocess.run([os.path.abspath(UPLOADER), "description", workshop_id, live_file],
                             cwd=os.path.dirname(os.path.abspath(UPLOADER)), stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    if fetched.returncode or not os.path.exists(live_file):
        print("Workshop description: could not read the live text, leaving it alone")
        return False
    live = normal(open(live_file, encoding="utf-8").read())
    ours = [normal(open(description, encoding="utf-8").read())]
    if previous_tag:
        shown = subprocess.run(["git", "show", "%s:publish/workshop-description.txt" % previous_tag],
                               stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
        if shown.returncode == 0:
            ours.append(normal(shown.stdout.decode("utf-8")))
    if live in ours:
        os.remove(live_file)
        return live != ours[0]
    print("Workshop description: the live text was edited on Steam; NOT overwriting it.")
    print("  saved as %s; merge it into workshop-description.txt, then upload with WorkshopUpload update <id> --description-file" % live_file)
    return False


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--dry-run", action="store_true", help="build and package only")
    ap.add_argument("--notes", help="release notes (default: the CHANGELOG.md section, else generated from the commits)")
    ap.add_argument("--workshop-id", help="Workshop item to update (default: publish/workshop-id.txt)")
    ap.add_argument("--no-workshop", action="store_true", help="GitHub release only")
    ap.add_argument("--changenote", help="Workshop change note (default: the CHANGELOG.md section, else version + shipped commit subjects)")
    a = ap.parse_args()

    projects = glob.glob(os.path.join("src", "*", "*.csproj"))
    if len(projects) != 1:
        sys.exit("expected exactly one src/<Mod>/<Mod>.csproj under %s" % os.getcwd())
    project_dir = os.path.dirname(projects[0])
    mod = os.path.splitext(os.path.basename(projects[0]))[0]
    info = open(os.path.join(project_dir, "mod_info.yaml"), encoding="utf-8").read()
    version = re.search(r"^version:\s*\"?([0-9][^\s\"]*)", info, re.M).group(1)
    tag = "v" + version
    workshop_id, how = (a.workshop_id, "file") if a.workshop_id else (None, "unlisted")
    if not a.no_workshop and not a.dry_run and not workshop_id:
        workshop_id, how = find_workshop_id()
        if how not in ("file", "listed", "unlisted"):
            sys.exit("%s; fix that or pass --no-workshop" % how)
        print("Workshop: " + ("item %s (%s)" % (workshop_id, how) if workshop_id else "not among your published items, skipping"))

    if not a.dry_run:
        if run("git", "status", "--porcelain", "--untracked-files=no", capture=True):
            sys.exit("work tree is not clean; commit or stash first")
        run("git", "fetch", "--quiet", "--tags")
        if run("git", "tag", "--list", tag, capture=True):
            sys.exit("tag %s already exists; bump the version in mod_info.yaml and the csproj" % tag)
        if run("git", "rev-list", "--count", "@{upstream}..HEAD", capture=True) != "0":
            sys.exit("HEAD is not pushed; push first so the tag lands on a commit GitHub has")

    run("dotnet", "build", project_dir, "-c", "Release", "-v", "q", "--nologo", "-p:ModDeployFolder=none")
    dlls = glob.glob(os.path.join(project_dir, "bin", "Release", mod + ".dll"))
    if len(dlls) != 1:
        sys.exit("expected one built %s.dll, found %s" % (mod, dlls))

    content = os.path.join("publish", "content")
    shutil.rmtree(content, ignore_errors=True)
    os.makedirs(content)
    shutil.copy(dlls[0], content)
    for name in ("mod.yaml", "mod_info.yaml"):
        shutil.copy(os.path.join(project_dir, name), content)
    for folder in ("anim", "assets"):
        if os.path.isdir(os.path.join(project_dir, folder)):
            shutil.copytree(os.path.join(project_dir, folder), os.path.join(content, folder))
    preview = os.path.join("publish", "preview.png")
    staged_preview = os.path.join(content, "preview.png")
    if os.path.exists(preview):
        from PIL import Image
        image = Image.open(preview)
        if max(image.size) > 512:
            image.convert("RGB").resize((512, 512), Image.LANCZOS).save(staged_preview, optimize=True)
        else:
            shutil.copy(preview, staged_preview)

    zip_path = os.path.join("publish", mod + ".zip")
    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as z:
        for root, _, files in os.walk(content):
            for f in sorted(files):
                path = os.path.join(root, f)
                z.write(path, os.path.relpath(path, content).replace(os.sep, "/"))
        print("packaged %s: %s" % (zip_path, ", ".join(z.namelist())))
    if a.dry_run:
        return

    previous = subprocess.run(["git", "describe", "--tags", "--abbrev=0", "--match", "v[0-9]*"],
                              text=True, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL).stdout.strip()
    subjects = run("git", "log", "--format=%s", (previous + "..HEAD") if previous else "HEAD", "--",
                   "src", "publish/preview.png", capture=True).splitlines()
    subjects = [line for line in subjects if not line.startswith("Release v")]
    changelog = changelog_section(version)
    notes = a.notes or changelog
    if changelog:
        print("release notes: CHANGELOG.md section %s" % version)
    else:
        print("release notes: no CHANGELOG.md section for %s; using the commits" % version)
    if a.changenote or a.notes:
        changenote = a.changenote or a.notes
    elif changelog:
        changenote = workshop_note(version, changelog)
    else:
        changenote = tag + (": " + "; ".join(subjects[:8]) if previous and subjects else "")

    asset = os.path.join("publish", "%s-%s.zip" % (mod, version))
    shutil.copy(zip_path, asset)
    try:
        run("git", "tag", "-a", tag, "-m", "%s %s" % (mod, version))
        run("git", "push", "origin", tag)
        notes = ["--notes", notes] if notes else ["--generate-notes"]
        run("gh", "release", "create", tag, asset, "--title", "%s %s" % (mod, version), "--verify-tag", *notes)
    finally:
        os.remove(asset)

    if workshop_id and how == "listed":
        id_file = os.path.join("publish", "workshop-id.txt")
        print(workshop_id, file=open(id_file, "w"))
        run("git", "add", id_file)
        run("git", "commit", "-m", "Record the Workshop item id", "--", id_file)
        run("git", "push")
    if workshop_id:
        args = [os.path.abspath(UPLOADER), "update", workshop_id, os.path.abspath(zip_path)]
        if os.path.exists(staged_preview):
            args.append(os.path.abspath(staged_preview))
        description = os.path.join("publish", "workshop-description.txt")
        if os.path.exists(description) and description_is_ours(workshop_id, description, previous):
            args += ["--description-file", os.path.abspath(description)]
        args += ["--changenote", changenote]
        subprocess.run(args, check=True, cwd=os.path.dirname(os.path.abspath(UPLOADER)))


if __name__ == "__main__":
    main()
