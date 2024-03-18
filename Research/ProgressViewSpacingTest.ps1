[CmdletBinding()]
param (
	[Parameter()]
	[int]
	$MaxWidth = -1
)
if($MaxWidth -gt 0){
	$psstyle.Progress.MaxWidth=$MaxWidth
}
$i = 0 # counter
$percents = 5, 55
$times = @([int]::maxvalue, 10, 100, 1000)
$uniqueCharLength = 1 # space to make the last character something different (so we know it was cut off)

while ($true) {
	# read key and change view if possible
	if ([console]::KeyAvailable) {
		$keyinfo = [console]::ReadKey($true)
		if ($keyinfo.KeyChar -eq 'm') {
			$view = 'Minimal'
		} elseif ($keyinfo.KeyChar -eq 'c') {
			$view = 'Classic'
		}
	}
	try {
		$PSStyle.Progress.View = $view
	} catch {
	}

	$i++
	$percent = $percents[$i % ($percents.Count)]
	$time = $times[$i % ($times.Count)]

	Clear-Host

	$buffer = $Host.UI.RawUI.BufferSize
	$maxWidth = [math]::min($buffer.Width, $psstyle.Progress.MaxWidth)

	$timeString = [timespan]::FromSeconds($time).tostring()

	$percentDigits = [math]::floor([math]::log10($percent) + 1)
	$percentWidth = $percentDigits + 1 # with percent sign

	if ($psstyle.Progress.view -eq 'minimal') {
		$viewName = 'minimal'
		$width = [math]::min($buffer.Width, $psstyle.Progress.MaxWidth)
		[int]$halfwidth = [math]::floor([math]::floor(0.5 * $width))
		$progressWidth = $width - ($halfwidth + 1 + 2)
		$progressInside = $progressWidth - 2

		$timeDigits = [math]::floor([math]::log10($time) + 1)
		$timeWidth = $timeDigits + 1

		$statusWidth = $progressInside - $timeWidth - 1 - $uniqueCharLength
		$activityWidth = $halfwidth - $uniqueCharLength

		$status = 'S' * $statusWidth + '!'
		$activity = 'A' * $activityWidth + '!'

		Write-Progress -Activity $activity -percent $percent -seco $time -Status $status
	} else {
		$width = $buffer.Width

		if ($buffer.Height -ge 18) {
			$viewName = 'classicfull'
			$statusWidth = $width - 4 - $uniqueCharLength # 3 padding minus 1 to make space for unique character
		} else {
			$viewName = 'classicsmall'
			$statusWidth = $width - 4 - $uniqueCharLength - $percentWidth - 1 - ($timeString.length) - 1  # like above but with space for percents and time
		}

		$activityWidth = $width - 1 - $uniqueCharLength  # 1 padding minus 1 to make space for unique character
		$curretnWidth = $width - 4 - $uniqueCharLength # 3 padding minus 1 to make space for unique character

		$activity = 'A' * $activityWidth + '!' # 1 padding minus 1 to make space for unique character
		$status = 'S' * $statusWidth + '!' # 3 padding minus 1 to make space for unique character
		$current = 'C' * $curretnWidth + '!' # 3 padding minus 1 to make space for unique character

		Write-Progress -Activity $activity -percent $percent -seco $time -Status $status -current $current
	}


	Write-Host "`rB: $buffer; Max: $maxWidth; Time: $timeString; status: $statusWidth; View: $viewname" -non
	Start-Sleep -mi 500
}
