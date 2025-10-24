import argparse
from pathlib import Path
import numpy as np
import pandas as pd

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

COLUMN_ALIASES = {
    "timestamp":     ["Timestamp", "timestamp", "timeStamp"],
    "session_id":    ["sessionId", "session_id", "SessionID", "sessionID"],
    "level_id":      ["levelId", "level_id", "LevelID", "levelID", "scene", "Scene"],
    "shots_fired":   ["shotsFired", "shots_fired", "shots", "Shots"],
    "correct_hits":  ["correctHits", "correct_hits", "hits", "Hits"],
    "time_taken_s":  ["timeTakenSec", "time_taken_s", "time_seconds", "time", "Time", "duration", "Duration"],
    "pauses":        ["pauseClicks", "pauses", "PauseClicks", "pausesCount"],
    "completed":     ["completed", "isWin", "win", "success", "Success"],
    "accuracy_pct":  ["accuracyPercent", "accuracy_pct", "accuracyPercentile"],
}

def pick_col(df: pd.DataFrame, candidates):
    for c in candidates:
        if c in df.columns:
            return c
    return None

def load_and_harmonize(csv_path: Path) -> pd.DataFrame:
    df = pd.read_csv(csv_path)

    colmap = {k: pick_col(df, v) for k, v in COLUMN_ALIASES.items()}

    # Required minimal set (accuracy can be computed if missing)
    required_min = ["session_id", "level_id", "shots_fired", "correct_hits", "time_taken_s", "pauses", "completed"]
    missing = [k for k in required_min if colmap[k] is None]
    if missing:
        raise ValueError(
            "Missing required columns (provide any of the accepted aliases): "
            + ", ".join(missing)
        )

    nd = pd.DataFrame()
    nd["session_id"]   = df[colmap["session_id"]]
    nd["level_id"]     = df[colmap["level_id"]].astype(str)
    nd["shots_fired"]  = pd.to_numeric(df[colmap["shots_fired"]], errors="coerce")
    nd["correct_hits"] = pd.to_numeric(df[colmap["correct_hits"]], errors="coerce")
    nd["time_taken_s"] = pd.to_numeric(df[colmap["time_taken_s"]], errors="coerce")
    nd["pauses"]       = pd.to_numeric(df[colmap["pauses"]], errors="coerce")
    nd["completed"]    = pd.to_numeric(df[colmap["completed"]], errors="coerce").clip(0,1).fillna(0).astype(int)

    # Accuracy (prefer provided)
    if colmap["accuracy_pct"] is not None:
        nd["accuracy_pct"] = pd.to_numeric(df[colmap["accuracy_pct"]], errors="coerce")
    else:
        nd["accuracy_pct"] = np.where(nd["shots_fired"] > 0,
                                      100.0 * nd["correct_hits"] / nd["shots_fired"],
                                      0.0)
    nd["accuracy_pct"] = nd["accuracy_pct"].clip(0, 100).fillna(0)

    # Drop rows with critical NaNs
    nd = nd.dropna(subset=["shots_fired","correct_hits","time_taken_s","pauses"])

    nd["outcome"] = np.where(nd["completed"]==1, "Win", "Loss")
    return nd

# ---- Plot helpers -----------------------------------------------------------
def ensure_plot_dir(out_dir: Path):
    out_dir.mkdir(parents=True, exist_ok=True)

def placeholder_plot(title: str, path: Path):
    plt.figure()
    plt.text(0.5, 0.5, "No data to plot", ha="center", va="center", fontsize=14)
    plt.title(title)
    plt.axis("off")
    plt.tight_layout()
    plt.savefig(path, dpi=200)
    plt.close()

def plot_accuracy_vs_completion(df, out_dir):
    path = out_dir / "accuracy_vs_completion_scatter.png"
    if len(df) == 0:
        placeholder_plot("Hit Accuracy vs. Outcome", path)
        return
    plt.figure()
    x = df["completed"].values + (np.random.rand(len(df)) - 0.5) * 0.05
    y = df["accuracy_pct"].values
    plt.scatter(x, y)
    plt.yticks(range(0, 101, 10))
    plt.xticks([0,1], ["Loss","Win"])
    plt.xlabel("Outcome")
    plt.ylabel("Hit Accuracy (%)")
    plt.title("Hit Accuracy vs. Outcome")
    plt.grid(True, axis="y", alpha=0.3)
    plt.tight_layout()
    plt.savefig(path, dpi=200)
    plt.close()

def plot_completion_rate(df, out_dir):
    path = out_dir / "completion_rate_bar.png"
    plt.figure()
    if len(df) == 0:
        plt.bar(["Loss","Win"], [0,0])
    else:
        rates = df["completed"].value_counts(normalize=True).reindex([0,1]).fillna(0) * 100.0
        plt.bar(["Loss","Win"], [rates.loc[0], rates.loc[1]])
    plt.ylabel("Percentage of Sessions (%)")
    plt.title("Completion Rate")
    plt.tight_layout()
    plt.savefig(path, dpi=200)
    plt.close()

def plot_time_by_outcome(df, out_dir):
    path = out_dir / "time_taken_by_outcome_box.png"
    plt.figure()
    if len(df) == 0:
        placeholder_plot("Time Taken by Outcome", path)
        return
    data = [df.loc[df["completed"]==0, "time_taken_s"], df.loc[df["completed"]==1, "time_taken_s"]]
    plt.boxplot(data, labels=["Loss","Win"], showfliers=True)
    plt.ylabel("Time Taken (s)")
    plt.title("Time Taken by Outcome")
    plt.tight_layout()
    plt.savefig(path, dpi=200)
    plt.close()

def plot_pauses_hist(df, out_dir):
    path = out_dir / "pauses_hist.png"
    plt.figure()
    if len(df) == 0:
        placeholder_plot("Pause Frequency", path)
        return
    max_p = int(df["pauses"].max()) if len(df) else 0
    bins = range(0, max_p + 2)
    plt.hist(df["pauses"], bins=bins, align="left", rwidth=0.9)
    plt.xlabel("Pause Count")
    plt.ylabel("Number of Sessions")
    plt.title("Pause Frequency")
    plt.tight_layout()
    plt.savefig(path, dpi=200)
    plt.close()

def plot_per_level_accuracy_box(df, out_dir):
    path = out_dir / "per_level_accuracy_box.png"
    plt.figure()
    if len(df) == 0:
        placeholder_plot("Per-Level Accuracy Distribution", path)
        return
    levels = sorted(df["level_id"].astype(str).unique())
    data = [df.loc[df["level_id"]==lvl, "accuracy_pct"] for lvl in levels]
    plt.boxplot(data, labels=levels, showfliers=True)
    plt.ylabel("Hit Accuracy (%)")
    plt.title("Per-Level Accuracy Distribution")
    plt.xticks(rotation=30, ha="right")
    plt.tight_layout()
    plt.savefig(path, dpi=200)
    plt.close()

# ---- Summaries & diagnostics ------------------------------------------------
def export_summary_tables(df: pd.DataFrame, out_dir: Path):
    per_level = (
        df.groupby("level_id", dropna=False)
          .agg(
              sessions=("session_id","nunique"),
              completion_rate_pct=("completed", lambda s: 100.0*s.mean() if len(s) else 0.0),
              mean_accuracy_pct=("accuracy_pct","mean"),
              median_accuracy_pct=("accuracy_pct","median"),
              mean_time_s=("time_taken_s","mean"),
              median_time_s=("time_taken_s","median"),
              mean_pauses=("pauses","mean")
          )
          .reset_index()
          .sort_values("level_id", key=lambda s: s.astype(str))
    )
    global_summary = pd.DataFrame([{
        "sessions": df["session_id"].nunique(),
        "completion_rate_pct": 100.0 * df["completed"].mean() if len(df) else 0.0,
        "mean_accuracy_pct": df["accuracy_pct"].mean() if len(df) else 0.0,
        "median_accuracy_pct": df["accuracy_pct"].median() if len(df) else 0.0,
        "mean_time_s": df["time_taken_s"].mean() if len(df) else 0.0,
        "median_time_s": df["time_taken_s"].median() if len(df) else 0.0,
        "mean_pauses": df["pauses"].mean() if len(df) else 0.0,
    }])
    per_level.to_csv(out_dir / "summary_per_level.csv", index=False)
    global_summary.to_csv(out_dir / "summary_global.csv", index=False)

def write_diagnostics(df: pd.DataFrame, out_dir: Path):
    lines = []
    lines.append(f"Rows after cleaning: {len(df)}")
    lines.append(f"Unique sessions: {df['session_id'].nunique()}")
    levels = ", ".join(sorted(map(str, df['level_id'].unique()))) if len(df) else "(none)"
    lines.append(f"Levels: {levels}")
    if len(df) >= 2 and df["accuracy_pct"].std(ddof=0) > 0:
        corr_acc = np.corrcoef(df["completed"], df["accuracy_pct"])[0,1]
        lines.append(f"Corr(completed, accuracy_pct): {corr_acc:.3f}")
    else:
        lines.append("Corr(completed, accuracy_pct): n/a")
    if len(df) >= 2 and df["time_taken_s"].std(ddof=0) > 0:
        corr_time = np.corrcoef(df["completed"], df["time_taken_s"])[0,1]
        lines.append(f"Corr(completed, time_taken_s): {corr_time:.3f}")
    else:
        lines.append("Corr(completed, time_taken_s): n/a")
    (out_dir / "diagnostics.txt").write_text("\n".join(lines), encoding="utf-8")

# ---- Main -------------------------------------------------------------------
def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--csv", required=True, help="Path to analytics CSV exported from Google Sheets")
    ap.add_argument("--out", default="out_charts", help="Output directory for charts and summaries")
    args = ap.parse_args()

    out_dir = Path(args.out)
    ensure_plot_dir(out_dir)

    df = load_and_harmonize(Path(args.csv))
    (out_dir / "cleaned_analytics.csv").write_text(df.to_csv(index=False), encoding="utf-8")

    # Plots (always produce files; placeholders if empty)
    plot_accuracy_vs_completion(df, out_dir)
    plot_completion_rate(df, out_dir)
    plot_time_by_outcome(df, out_dir)
    plot_pauses_hist(df, out_dir)
    plot_per_level_accuracy_box(df, out_dir)

    # Summaries + diag
    export_summary_tables(df, out_dir)
    write_diagnostics(df, out_dir)

    print("Wrote outputs to:", out_dir.resolve())

if __name__ == "__main__":
    main()
