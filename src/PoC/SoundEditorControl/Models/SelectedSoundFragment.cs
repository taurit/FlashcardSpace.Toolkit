using System;

namespace SoundEditorControl.Models;
public record SelectedSoundFragment(TimeSpan BeginningShift, TimeSpan EndShift, TimeSpan BeginningShiftRelativeToInitial, TimeSpan EndShiftRelativeToInitial);
