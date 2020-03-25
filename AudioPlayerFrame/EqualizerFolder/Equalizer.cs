using NAudio.Dsp;
using NAudio.Wave;

namespace AudioPlayer
{
    /// <summary>
    /// </summary>
    public class Equalizer : ISampleProvider
    {
        public readonly ISampleProvider sourceProvider; //stream provider
        public readonly EqualizerBand[] bands; //array of equalizer bands

        //filter through which stream passes before getting to audio output
        //only god and creator of NAudio knows how it works
        public readonly BiQuadFilter[,] filters; 
        public readonly int channels; //number of channels
        public readonly int bandCount; //number of redactable bands
        public bool updated; //check if it has already been updated

        public Equalizer(ISampleProvider sourceProvider, EqualizerBand[] bands)
        {
            this.sourceProvider = sourceProvider;
            this.bands = bands;
            channels = sourceProvider.WaveFormat.Channels;
            bandCount = bands.Length;
            filters = new BiQuadFilter[channels,bands.Length];
            CreateFilters();
        }

        //on creation of this class filters are being created
        //on update PeakingEQ of each channel in stream is being updated for each band
        private void CreateFilters() 
        {
            for (int bandIndex = 0; bandIndex < bandCount; bandIndex++) //do this for each band
            {
                var band = bands[bandIndex];
                for (int n = 0; n < channels; n++) //for each channel in stream do something with current band
                {
                    //TODO: figure out how PeakingEQ works
                    if (filters[n, bandIndex] == null)
                        filters[n, bandIndex] = BiQuadFilter.PeakingEQ(sourceProvider.WaveFormat.SampleRate, band.Frequency, band.Bandwidth, band.Gain); //magic
                    else
                        filters[n, bandIndex].SetPeakingEq(sourceProvider.WaveFormat.SampleRate, band.Frequency, band.Bandwidth, band.Gain); //more magic
                }
            }
        }

        public void Update()
        {
            updated = true;
            CreateFilters();
        }

        public WaveFormat WaveFormat { get { return sourceProvider.WaveFormat; } } //format of (??)waves(??) of stream 

        //Don't know what this does, it is never referenced
        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = sourceProvider.Read(buffer, offset, count);

            if (updated)
            {
                CreateFilters();
                updated = false;
            }

            for (int n = 0; n < samplesRead; n++)
            {
                int ch = n % channels; 
                
                for (int band = 0; band < bandCount; band++)
                {
                    buffer[offset + n] = filters[ch, band].Transform(buffer[offset + n]);
                }
            }
            return samplesRead;
        }
    }
}