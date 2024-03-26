# Classic View
Every element in a different line
- Activity
  - Padding
    - 1 character from left
  - 1 line only
  - Cut off without anything
- Status
  - Padding
    - 4 characters from left
  - 1 line only (sometimes?)
  - Cut off without anything
- Progress Bar
  - Padding
    - 4 characters from left
 - 7 characters from right
- Time remaining
  - Padding
    - 4 characters from left
  - Dynamic format string
- Current operation
  - Padding
    - 4 characters from left
    - 1 empty line from Time remaining
   - 1 line only
   - Cut off without anything
   
# Minimal View
Everything in one line
- Activity
  - At most 50% (inclusive) of RawUI.BufferSize.Width, rounded down (UI 260 => activity 130; ui 261 => Activity 130)
  - Padding
    - 1 character from right
	  - Always present, NOT included in length limit
  - 1 line only
  - Cut off by replacing last character with '.'
- Progress Bar
    - 2 characters from right
	- includes '[' and ']'
  - Status
    - Cut off by replacing last character with '.'
  - Time remaining
    - Only seconds
  - Current operation
    - NOT PRESENT
