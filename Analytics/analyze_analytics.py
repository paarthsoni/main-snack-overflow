"""
Snack Overflow Analytics Visualization (Colorful Version, Fixed)

Usage:
  python analyze_analytics.py --csv analytics.csv --out out_charts
"""

import argparse
from pathlib import Path
import numpy as np
import pandas as pd

# Non-interactive backend so charts save even in terminals / VSCode
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt


# -------------------- Palette / Style --------------------
COLORS = {
    "blue": "#0072B2",
    "orange": "#E69F00",
    "green": "#009E73",
    "red": "#D55E00",
    "purple": "#CC79A7",
    "gray": "#999999",
    "yellow": "#F0E442",
}

plt.rcParams["axes.prop_cycle"] = matplotlib.cycler(color=[
    COLORS["blue"], COLORS["orange"], COLORS["green"],
    COLORS["red"], COLORS["purple"], COLORS["yellow"]
])
plt.rcParams["figure.dpi"] = 120
plt.rcParams["savefig.dpi"] = 200
plt.rcParams["axes.grid"] = True
plt.rcParams["grid.alpha"] = 0.25


# -------------------- Column Mapping --------------------
COLUMN_ALIASES = {
    "session_id": ["sessionId", "session_id", "SessionID"],
    "level_id": ["levelId", "level_id", "LevelID"],
    "shots_fired": ["shotsFired", "shots_fired"],
    "correct_hits": ["correctHits", "correct_hits"],
    "time_taken_s": ["timeTakenSec", "time_taken_s", "time_seconds"],
    "pauses": ["pauseClicks", "pauses"],
    "completed": ["completed", "isWin", "success"],
    "accuracy_pct": ["accuracyPercent", "accuracy_pct"],
}


def pick_col(df, options):
    for o in options:
        if o in df.columns:
            return o
    return None


def load_and_clean(csv_path: Path) -> pd.DataFrame:
    df = pd.read_csv(csv_path)
    colmap = {k: pick_col(df, v) for k, v in COLUMN_ALIASES.items()}

    req = ["session_id", "level_id", "shots_fired",
           "correct_hits", "time_taken_s", "pauses", "completed"]
    missing = [r for r in req if colmap[r] is None]
    if missing:
        raise ValueError(f"Missing columns: {missing}")

    nd = pd.DataFrame()
    nd["session_id"] = df[colmap["session_id"]]
    nd["level_id"] = df[colmap["level_id"]].astype(str)
    nd["shots_fired"] = pd.to_numeric(df[colmap["shots_fired"]], errors="coerce")
    nd["correct_hits"] = pd.to_numeric(df[colmap["correct_hits"]], errors="coerce")
    nd["time_taken_s"] = pd.to_numeric(df[colmap["time_taken_s"]], errors="coerce")
    nd["pauses"] = pd.to_numeric(df[colmap["pauses"]], errors="coerce")
    nd["completed"] = pd.to_numeric(df[colmap["completed"]], errors="coerce").clip(0, 1).fillna(0).astype(int)

    nd["accuracy_pct"] = np.where(
        nd["shots_fired"] > 0,
        (nd["correct_hits"] / nd["shots_fired"]) * 100.0,
        0.0
    )
    nd["accuracy_pct"] = nd["accuracy_pct"].clip(0, 100).fillna(0)

    nd["accuracy_pct"] = nd["accuracy_pct"].clip(0, 100).fillna(0)
    nd["outcome"] = np.where(nd["completed"] == 1, "Win", "Loss")

    return nd.dropna(subset=["shots_fired", "correct_hits", "time_taken_s", "pauses"])


# -------------------- Plot Helpers --------------------
def placeholder_plot(title, path):
    plt.figure()
    plt.text(0.5, 0.5, "No Data Available", ha="center", va="center",
             fontsize=14, color=COLORS["red"])
    plt.title(title)
    plt.axis("off")
    plt.tight_layout()
    plt.savefig(path)
    plt.close()


def plot_accuracy_vs_completion(df, out_dir: Path):
    path = out_dir / "accuracy_vs_completion_bar.png"
    if len(df) == 0:
        placeholder_plot("Average Hit Accuracy by Outcome", path)
        return
    
    avg = (
        df.groupby("outcome")["accuracy_pct"]
          .mean()
          .reindex(["Loss", "Win"])
          .fillna(0)
    )

    plt.figure()
    bars = plt.bar(
        avg.index,
        avg.values,
        color=[COLORS["red"], COLORS["green"]],
        edgecolor="black",
        alpha=0.8,
    )

    for bar in bars:
        height = bar.get_height()
        plt.text(
            bar.get_x() + bar.get_width() / 2,
            height + 1,
            f"{height:.1f}%",
            ha="center",
            va="bottom",
            fontsize=10,
            color="black",
            fontweight="bold"
        )

    plt.ylabel("Average Hit Accuracy (%)")
    plt.title("Average Hit Accuracy by Outcome")
    plt.ylim(0, 100)
    plt.tight_layout()
    plt.savefig(path)
    plt.close()


def plot_completion_rate(df, out_dir: Path):
    path = out_dir / "completion_rate_bar.png"
    plt.figure()
    if len(df) == 0:
        vals = [0, 0]
    else:
        rates = df["completed"].value_counts(normalize=True).reindex([0, 1]).fillna(0) * 100.0
        vals = [rates.loc[0], rates.loc[1]]
    ax = plt.gca()
    bars = ax.bar(["Loss", "Win"], vals, color=[COLORS["red"], COLORS["green"]])
    try:
        ax.bar_label(bars, fmt="%.1f%%", padding=4)
    except Exception:
        for rect, v in zip(bars, vals):
            ax.text(rect.get_x() + rect.get_width()/2, v + 2, f"{v:.1f}%",
                    ha="center", va="bottom", fontsize=10)
    ax.set_ylabel("Percentage of Sessions (%)")
    ax.set_title("Completion Rate")
    plt.tight_layout()
    plt.savefig(path)
    plt.close()


def plot_time_by_outcome(df, out_dir: Path):
    path = out_dir / "time_taken_by_outcome_box.png"
    if len(df) == 0:
        placeholder_plot("Time Taken by Outcome", path)
        return
    data = [df.loc[df["completed"] == 0, "time_taken_s"],
            df.loc[df["completed"] == 1, "time_taken_s"]]
    plt.figure()
    bp = plt.boxplot(data, labels=["Loss", "Win"], patch_artist=True)
    for patch, color in zip(bp["boxes"], [COLORS["red"], COLORS["green"]]):
        patch.set_facecolor(color)
        patch.set_alpha(0.4)
    plt.ylabel("Time Taken (s)")
    plt.title("Time Taken by Outcome")
    plt.tight_layout()
    plt.savefig(path)
    plt.close()


def plot_pauses_hist(df, out_dir: Path):
    path = out_dir / "pauses_hist.png"
    if len(df) == 0:
        placeholder_plot("Pause Frequency", path)
        return
    plt.figure()
    bins = np.arange(-0.5, df["pauses"].max() + 1.5, 1)
    plt.hist(df["pauses"], bins=bins, color=COLORS["purple"], edgecolor="white", rwidth=0.9)
    plt.xlabel("Pause Count")
    plt.ylabel("Number of Sessions")
    plt.title("Pause Frequency")
    plt.tight_layout()
    plt.savefig(path)
    plt.close()


def plot_per_level_accuracy(df, out_dir: Path):
    path = out_dir / "per_level_accuracy_box.png"
    if len(df) == 0:
        placeholder_plot("Per-Level Accuracy Distribution", path)
        return
    levels = sorted(df["level_id"].unique())
    data = [df.loc[df["level_id"] == lvl, "accuracy_pct"] for lvl in levels]
    plt.figure()
    bp = plt.boxplot(data, labels=levels, patch_artist=True)
    palette = [COLORS["blue"], COLORS["orange"], COLORS["green"],
               COLORS["red"], COLORS["purple"], COLORS["yellow"]]
    for i, box in enumerate(bp["boxes"]):
        box.set_facecolor(palette[i % len(palette)])
        box.set_alpha(0.5)
    plt.ylabel("Hit Accuracy (%)")
    plt.title("Per-Level Accuracy Distribution")
    plt.xticks(rotation=25)
    plt.tight_layout()
    plt.savefig(path)
    plt.close()


# -------------------- Summaries --------------------
def export_summary_tables(df, out_dir: Path):
    per_level = (
        df.groupby("level_id", dropna=False)
          .agg(
              sessions=("session_id", "nunique"),
              completion_rate_pct=("completed", lambda s: 100 * s.mean() if len(s) else 0),
              mean_accuracy_pct=("accuracy_pct", "mean"),
              median_accuracy_pct=("accuracy_pct", "median"),
              mean_time_s=("time_taken_s", "mean"),
              median_time_s=("time_taken_s", "median"),
              mean_pauses=("pauses", "mean"),
          )
          .reset_index()
          .sort_values("level_id")
    )
    global_summary = pd.DataFrame([{
        "sessions": df["session_id"].nunique(),
        "completion_rate_pct": 100 * df["completed"].mean() if len(df) else 0,
        "mean_accuracy_pct": df["accuracy_pct"].mean() if len(df) else 0,
        "median_accuracy_pct": df["accuracy_pct"].median() if len(df) else 0,
        "mean_time_s": df["time_taken_s"].mean() if len(df) else 0,
        "median_time_s": df["time_taken_s"].median() if len(df) else 0,
        "mean_pauses": df["pauses"].mean() if len(df) else 0,
    }])
    per_level.to_csv(out_dir / "summary_per_level.csv", index=False)
    global_summary.to_csv(out_dir / "summary_global.csv", index=False)


def write_diagnostics(df, out_dir: Path):
    lines = []
    lines.append(f"Rows after cleaning: {len(df)}")
    lines.append(f"Unique sessions: {df['session_id'].nunique()}")
    levels = ", ".join(sorted(map(str, df['level_id'].unique()))) if len(df) else "(none)"
    lines.append(f"Levels: {levels}")
    if len(df) >= 2 and df["accuracy_pct"].std(ddof=0) > 0:
        corr_acc = np.corrcoef(df["completed"], df["accuracy_pct"])[0, 1]
        lines.append(f"Corr(completed, accuracy_pct): {corr_acc:.3f}")
    if len(df) >= 2 and df["time_taken_s"].std(ddof=0) > 0:
        corr_time = np.corrcoef(df["completed"], df["time_taken_s"])[0, 1]
        lines.append(f"Corr(completed, time_taken_s): {corr_time:.3f}")
    (out_dir / "diagnostics.txt").write_text("\n".join(lines), encoding="utf-8")


# -------------------- Main --------------------
def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--csv", required=True)
    ap.add_argument("--out", default="out_charts")
    args = ap.parse_args()

    out_dir = Path(args.out)
    out_dir.mkdir(parents=True, exist_ok=True)

    df = load_and_clean(Path(args.csv))
    df.to_csv(out_dir / "cleaned_analytics.csv", index=False)

    plot_accuracy_vs_completion(df, out_dir)
    plot_completion_rate(df, out_dir)
    plot_time_by_outcome(df, out_dir)
    plot_pauses_hist(df, out_dir)
    plot_per_level_accuracy(df, out_dir)

    export_summary_tables(df, out_dir)
    write_diagnostics(df, out_dir)

    print("✅ Charts and summaries saved in:", out_dir.resolve())


if __name__ == "__main__":
    main()
