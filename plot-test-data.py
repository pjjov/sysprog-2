#!/usr/bin/env python3

import sys
import pandas as pd
import matplotlib.pyplot as plt

# === CHECK ARGUMENT ===
if len(sys.argv) != 2:
    print(f"Usage: {sys.argv[0]} <input.csv>")
    sys.exit(1)

csv_file = sys.argv[1]

# === LOAD CSV (no header) ===
df = pd.read_csv(csv_file, header=None, names=["status", "timestamp", "elapsed"])

# === PARSE TIMESTAMP ===
# Assuming timestamp is in microseconds since epoch
df["timestamp"] = pd.to_datetime(df["timestamp"], unit="us", errors="coerce")

# === PARSE ELAPSED TIME ===
# Convert HH:MM:SS.ssssss to seconds
df["elapsed_seconds"] = pd.to_timedelta(df["elapsed"], errors="coerce").dt.total_seconds()

# Drop bad rows if any
df = df.dropna(subset=["timestamp", "elapsed_seconds"])

# Sort by time
df = df.sort_values("timestamp")

# === PLOT 1: Latency over time ===
plt.figure(figsize=(12, 6))
plt.plot(df["timestamp"], df["elapsed_seconds"], linestyle='-', marker='o', alpha=0.6)

plt.title("Response Time Over Time")
plt.xlabel("Timestamp")
plt.ylabel("Elapsed Time (seconds)")
plt.xticks(rotation=45)
plt.grid(True)
plt.tight_layout()
plt.show()