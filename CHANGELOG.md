# Changelog

## [1.1.0](https://github.com/Pedrokostam/WriteProgressPlus/tree/1.1.0) - 2024-02-22 - *Full Throttle*

Fixed:

* Bars could remain on screen if the loop was very fast. It was caused by the throttling mechanism skipping the last update with RecordType set to true. Now the throttling cannot prevent the final update.

Improvements:

* Disabled the throttling mechanism on ConsoleHost for Powershell > 6, as those have their own throttling mechanism built-in.
* Added a dynamic parameter for Powershell below 6 to disable throttling.
* Slightly increased throttling performance.
* It is now possible to specify an ICollection for TotalCount - it will automatically get its count

## [1.0.0](https://github.com/Pedrokostam/WriteProgressPlus/tree/1.0.0) - 2024-02-21 - *Genesis*

First full release

Features:

* Item formatting works with both script and properties, including wildcards
* Progress bars are properly nested when specifying ParentId
* State can be preserved between commands
* Every parameter has been documented
* Multiple examples in the help document
* Pipeline pass-thru works correctly

Style:

* Most methods have comment-based documentation
* Names are descriptive

Future Goals:

* Ability to provide a collection as well as int to TotalCount (QoL for user)
