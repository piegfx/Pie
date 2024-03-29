using System;
using System.Threading.Tasks;
using Pie.Audio;
using Pie.Audio.Stream;

namespace Common;

public class StreamPlayer : IDisposable
{
    private AudioDevice _device;
    
    private Stream _stream;
    private AudioFormat _format;

    private byte[] _buffer;
    
    private AudioBuffer[] _buffers;
    private int _currentBuffer;

    private ushort _voice;

    private object _lock = new object();
    
    public StreamPlayer(AudioDevice device, string path)
    {
        _device = device;
        
        _stream = Stream.FromFile(path);
        _format = _stream.Format;

        _buffer = new byte[_format.SampleRate * _format.DataType.Bytes()];

        _buffers = new AudioBuffer[2];

        for (int i = 0; i < _buffers.Length; i++)
        {
            _stream.GetBuffer(ref _buffer);
            _buffers[i] = _device.CreateBuffer(new BufferDescription(_format), _buffer);
        }
        
        _device.BufferFinished += DeviceOnBufferFinished;

        _voice = ushort.MaxValue;
    }

    private void DeviceOnBufferFinished(AudioBuffer buffer, ushort voice)
    {
        if (voice != _voice)
            return;

        // Get a new buffer in a separate thread.
        // The buffer finished callback is called in the same thread as whichever thread ReadBuffer is called (which in
        // this case is the SDL audio thread). This can mean however that tasks can stall the audio pipeline if they
        // take too long to process. Running this in a separate thread ensures this cannot happen.
        Task.Run(() =>
        {
            lock (_lock)
            {
                ulong decoded = _stream.GetBuffer(ref _buffer);
                if (decoded < _format.SampleRate)
                {
                    _stream.Restart();
                    _device.UpdateBuffer(_buffers[_currentBuffer], _format, _buffer[..((int) decoded * _format.DataType.Bytes())]);
                }
                else
                    _device.UpdateBuffer(_buffers[_currentBuffer], _format, _buffer);
                
                _device.QueueBuffer(_buffers[_currentBuffer], voice);

                _currentBuffer++;
                if (_currentBuffer >= _buffers.Length)
                    _currentBuffer = 0;
            }
        });
    }

    public void Play(ushort voice, PlayProperties properties)
    {
        _voice = voice;

        // Force looping to be enabled - this means that if the buffer callback stalls for whatever reason, mixr will
        // continue to play the last buffer in memory until a new buffer is queued.
        // If we don't disable looping, the song will immediately stop playing if this event occurs.
        // This event is unlikely but it is no harm to enable this.
        properties.Looping = true;
        
        _device.PlayBuffer(_buffers[0], voice, properties);
        for (int i = 1; i < _buffers.Length; i++)
            _device.QueueBuffer(_buffers[i], voice);
    }

    public void Dispose()
    {
        _device.SetVoiceState(_voice, PlayState.Stopped);
        _device.BufferFinished -= DeviceOnBufferFinished;
        
        foreach (AudioBuffer buffer in _buffers)
            _device.DestroyBuffer(buffer);
        
        _stream.Dispose();
    }
}