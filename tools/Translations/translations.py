"""Translation templates and checks for every isochronous mod, from their built DLLs.

    python tools/Translations/translations.py [mod-folder ...] [--build]

For each mod repo next to this one (or the ones named) whose csproj sets UseTranslations,
runs StringsTemplate on src/<Mod>/bin/Release/<Mod>.dll: rewrites src/<Mod>/translations/<Mod>.pot
from the strings compiled into the DLL and checks every <locale>.po there against it (missing,
stale, empty, obsolete entries, and entries the game would refuse). --build runs a Release build
of each mod first (no deploy). Exit code 1 when any mod has a problem.

Build the DLL before running this without --build: the template reflects the DLL, not the source.
"""
import glob
import os
import re
import subprocess
import sys

COMMON = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
ROOT = os.path.dirname(COMMON)
TOOL = os.path.join(COMMON, "tools", "Translations", "StringsTemplate")


def game_managed_dir():
    env = os.environ.get("ONI_GAME_FOLDER")
    candidates = [env] if env else []
    candidates += [r"F:\SteamLibrary\steamapps\common\OxygenNotIncluded",
                   r"C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded"]
    for folder in candidates:
        managed = os.path.join(folder, "OxygenNotIncluded_Data", "Managed")
        if os.path.isfile(os.path.join(managed, "Assembly-CSharp.dll")):
            return managed
    sys.exit("game not found; set ONI_GAME_FOLDER")


def mod_projects(names):
    folders = [os.path.join(ROOT, n) for n in names] if names else sorted(glob.glob(os.path.join(ROOT, "*")))
    for folder in folders:
        for csproj in glob.glob(os.path.join(folder, "src", "*", "*.csproj")):
            text = open(csproj, encoding="utf-8").read()
            if re.search(r"<UseTranslations>\s*true\s*</UseTranslations>", text):
                yield folder, os.path.dirname(csproj), os.path.splitext(os.path.basename(csproj))[0]


def main():
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    build = "--build" in sys.argv
    managed = game_managed_dir()
    subprocess.run(["dotnet", "build", TOOL, "-c", "Release", "--nologo", "-v", "q"], check=True)
    tool_dll = os.path.join(TOOL, "bin", "Release", "net8.0", "StringsTemplate.dll")
    problems = 0
    found = False
    for folder, project, mod in mod_projects(args):
        found = True
        print("==", os.path.basename(folder))
        if build:
            subprocess.run(["dotnet", "build", project, "-c", "Release", "--nologo", "-v", "q", "-p:ModDeployFolder=none"], check=True)
        dll = os.path.join(project, "bin", "Release", mod + ".dll")
        if not os.path.isfile(dll):
            print("   no Release build at", dll, "(build it, or pass --build)")
            problems += 1
            continue
        result = subprocess.run(["dotnet", tool_dll, managed, dll, os.path.join(project, "translations")])
        if result.returncode != 0:
            problems += 1
    if not found:
        sys.exit("no mod with <UseTranslations>true</UseTranslations> found")
    sys.exit(1 if problems else 0)


if __name__ == "__main__":
    main()
