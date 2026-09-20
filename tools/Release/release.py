#!/usr/bin/env python3
"""Build a mod, package it, tag the commit and publish a GitHub release, in one step.

    python common/tools/Release/release.py [--dry-run] [--notes "text"] [--workshop-id ID [--changenote "text"]]

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
  6. With --workshop-id, also uploads the same zip and preview to that Workshop item.

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


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--dry-run", action="store_true", help="build and package only")
    ap.add_argument("--notes", help="release notes (default: generated from the commits since the last tag)")
    ap.add_argument("--workshop-id", help="also update this Steam Workshop item")
    ap.add_argument("--changenote", help="Workshop change note (default: the release notes or the tag)")
    a = ap.parse_args()

    projects = glob.glob(os.path.join("src", "*", "*.csproj"))
    if len(projects) != 1:
        sys.exit("expected exactly one src/<Mod>/<Mod>.csproj under %s" % os.getcwd())
    project_dir = os.path.dirname(projects[0])
    mod = os.path.splitext(os.path.basename(projects[0]))[0]
    info = open(os.path.join(project_dir, "mod_info.yaml"), encoding="utf-8").read()
    version = re.search(r"^version:\s*\"?([0-9][^\s\"]*)", info, re.M).group(1)
    tag = "v" + version

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

    asset = os.path.join("publish", "%s-%s.zip" % (mod, version))
    shutil.copy(zip_path, asset)
    try:
        run("git", "tag", "-a", tag, "-m", "%s %s" % (mod, version))
        run("git", "push", "origin", tag)
        notes = ["--notes", a.notes] if a.notes else ["--generate-notes"]
        run("gh", "release", "create", tag, asset, "--title", "%s %s" % (mod, version), "--verify-tag", *notes)
    finally:
        os.remove(asset)

    if a.workshop_id:
        args = [os.path.abspath(UPLOADER), "update", a.workshop_id, os.path.abspath(zip_path)]
        if os.path.exists(staged_preview):
            args.append(os.path.abspath(staged_preview))
        description = os.path.join("publish", "workshop-description.txt")
        if os.path.exists(description):
            args += ["--description-file", os.path.abspath(description)]
        args += ["--changenote", a.changenote or a.notes or tag]
        subprocess.run(args, check=True, cwd=os.path.dirname(os.path.abspath(UPLOADER)))


if __name__ == "__main__":
    main()
