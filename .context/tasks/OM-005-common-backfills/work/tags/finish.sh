#!/usr/bin/env bash
# finish.sh <node> [dry|write]: close every open pass of the node, validate each, then dry-run or write the round.
set -u
cd /c/dev/ONI-Mods/common || exit 1
W=.context/tasks/OM-005-common-backfills/work/tags
node_path=$1; n=$(basename "$node_path"); mode=${2:-dry}; R=${ROUND:+-$ROUND}; RT=${ROUND:-r1}
for c in $(node -e 'console.log(require(process.argv[1]).map(c=>c.id).join(" "))' "$PWD/$W/chunks-$n$R.json"); do
  if [ -f "$W/tagout-$c.partial" ]; then node $W/tagwork.mjs complete tagout-$c > /dev/null || exit 1; fi
  node $W/tagwork.mjs check "$node_path" $c | grep -q '"ok": true' || { echo "CHECK FAILED $c"; node $W/tagwork.mjs check "$node_path" $c; exit 1; }
done
node $W/tagwork.mjs $mode "$node_path" $RT
