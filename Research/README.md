# ProgressViewSpacingTest.ps1
This script will create a progress bar where every part of the bar is as filled as possible without clipping.

It works with classic and minimal view. Pressing 'c' when running swithes view to `Classic`; pressing 'm' switches it to `Minimal` (where applicable)

The script accept an integer parameter, which is the value that will be set to $PSStyle.Progress.MaxWidth (if it exists). Value below 40 not recommended.
