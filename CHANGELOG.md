# Changelog

## [1.2.1](https://github.com/Pedrokostam/WriteProgressPlus/tree/1.2.1) - 2024-04-08 - *One and done*

### Improvements

* The PowerShell version is now checked only once, during the import or on first call.
* Reduced number of calls on dynamic objects when checking `ProgressStyle`.

## [1.2.0](https://github.com/Pedrokostam/WriteProgressPlus/tree/1.2.0) - 2024-03-26 - *Stylish*

### Fixes

* Redone the check for throttling capability to make it work in less common cases:
  * The check now uses `PSVersionTable.PSVersion` instead of `Host.Version`.
    * Reason: `Host.Version` may be completely different from PowerShell version
  * The host name is not checked.
    * Reason: VS Code has a differently named Host but still throttles, which makes this check useless.

### Improvements

* For PowerShell >= 7.2, the module is now aware of which `ProgressStyle` is being used.
* The module now automatically trim all parts of the progress layout to ensure that they fit on a single line.
  * Status, CurrentOperation, Activity are affected.
  * The available space is determined with the current `ProgressStyle` in mind.
* Classic progress style now utilizes the CurrentOperation property (it displays the formatted item).

## [1.1.0](https://github.com/Pedrokostam/WriteProgressPlus/tree/1.1.0) - 2024-02-22 - *Full Throttle*

### Fixed

* Bars could remain on screen if the loop was very fast. It was caused by the throttling mechanism skipping the last update with RecordType set to true. Now the throttling cannot prevent the final update.

### Improvements

* Disabled the throttling mechanism on ConsoleHost for Powershell >= 6, as those have their own throttling mechanism built-in.
* Added a dynamic parameter for Powershell < 6 to disable throttling.
* Slightly increased throttling performance.
* It is now possible to specify an `ICollection` for TotalCount - it will automatically get its count

## [1.0.0](https://github.com/Pedrokostam/WriteProgressPlus/tree/1.0.0) - 2024-02-21 - *Genesis*

First full release

### Features

* Item formatting works with both script and properties, including wildcards
* Progress bars are properly nested when specifying ParentId
* State can be preserved between commands
* Every parameter has been documented
* Multiple examples in the help document
* Pipeline pass-thru works correctly

### Style

* Most methods have comment-based documentation
* Names are descriptive
