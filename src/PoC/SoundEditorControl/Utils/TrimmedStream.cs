using NAudio.Wave;

namespace SoundEditorControl.Utils;

internal class TrimmedStream : WaveStream
{
    private readonly long _endPosition;
    private readonly WaveStream _sourceStream;

    public TrimmedStream(WaveStream sourceStream, long startPosition, long endPosition)
    {
        _sourceStream = sourceStream;
        _endPosition = endPosition;
        _sourceStream.Position = startPosition;
    }

    public override long Length => _endPosition;

    public override long Position
    {
        get => _sourceStream.Position;
        set => _sourceStream.Position = value;
    }

    public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_sourceStream.Position + count > _endPosition) count = (int)(_endPosition - _sourceStream.Position);
        return _sourceStream.Read(buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        _sourceStream.Dispose();
        base.Dispose(disposing);
    }
}
