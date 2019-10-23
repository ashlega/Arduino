using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Media;

using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.Windows.Media;
using System.Timers;




public class Sound
{

    private WaveOutEvent outputDevice = null;
    private AudioFileReader audioFile = null;

    private float lastVolume = 0;

    private string fileName = null;
    float maxVolume = 0f;
    float minVolume = 0f;

    System.Timers.Timer tm;
    DateTime startTime;
    int tickDuration = 100;
    int ticksPerCycle = 100;

    bool ignoreSilence = false;
  
    public void ChangeSoundLevel(object sender, ElapsedEventArgs e)
    {
        var totalMilliseconds = (DateTime.Now - startTime).TotalMilliseconds;
        var multiplier = totalMilliseconds / (tickDuration * ticksPerCycle);
        var vol = minVolume + (maxVolume - minVolume) *Math.Abs((float)Math.Sin(Math.PI/2 * multiplier));
        SetVolume(vol);
    }

    public Sound(string fileName, float minVolume, float maxVolume, bool changingVolume, bool ignoreSilence)
    {
        this.ignoreSilence = ignoreSilence;
        this.maxVolume = maxVolume;
        this.minVolume = minVolume;
        this.fileName = fileName;
        if(changingVolume)
        {
            startTime = DateTime.Now;
            tm = new System.Timers.Timer();
            tm.Interval = tickDuration;
            tm.Elapsed += ChangeSoundLevel;
            tm.Start();
        }
    }


    private bool silence = false;
    public bool Silence
    {
        get
        {
            return silence;
        }
        set
        {
            if (ignoreSilence)
            {
                silence = false;
            }
            else if (value != silence)
            {
                silence = value;
                if (silence)
                {
                    if (audioFile != null) audioFile.Volume = 0;
                }
                else
                {
                    audioFile.Volume = lastVolume;
                }
            }
        }
    }

    public void Init(int deviceNumber)
    {

        //disposeWave();
        outputDevice = new WaveOutEvent();
        outputDevice.DeviceNumber = (int)deviceNumber;
        audioFile = new AudioFileReader(fileName);

        outputDevice.Init(audioFile);
        SetVolume(maxVolume);
        outputDevice.PlaybackStopped += PlaybackStopped;

    }

    public void Play()
    {
        if(outputDevice != null) outputDevice.Play();
    }

    private void PlaybackStopped(object sender, Object e)
    {
        audioFile.Position = 0;
        outputDevice.Play();
    }

    public void SetVolume(float volume)
    {
        if (audioFile != null)
        {
            lastVolume = volume;
            if (!silence)
            {
                audioFile.Volume = volume;
            }
        }
    }

    public void disposeWave()
    {
        if (outputDevice != null)
        {
            if (outputDevice.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                outputDevice.Stop();
                outputDevice.Dispose();
                outputDevice = null;
            }
        }
    }

}

public class SoundList : List<Sound>
{
    public void Silence(bool value)
    {
        foreach (var s in this)
        {
            s.Silence = value;
        }
    }

}

public class SerialTest
{


    private static string dataDelimiter = "|";
    static SoundList sl = new SoundList();

    public static string ParseResponse(string response)
    {
        int i = response.IndexOf(dataDelimiter);
        if (i > 0)
        {
            while (i > 0)
            {
                String currentData = response.Substring(0, i);
                Console.WriteLine(currentData);
                if (i + 1 < response.Length)
                {
                    response = response.Substring(i + 1);
                }
                else response = "";
                if (currentData.Contains("D:"))
                {
                    //Distance data
                }
                else if (currentData.Contains("MN:0"))
                {
                    sl.Silence(true);
                    Thread.Sleep(500);
                    
                }
                else if (currentData.Contains("MN:1"))
                {
                    sl.Silence(false);
                    Thread.Sleep(500);
                    
                }

                i = response.IndexOf(dataDelimiter);
            }
        }
        else
        {
            Thread.Sleep(2000);
        }
        
        return response;

    }
    public static void Main()
    {

        //ThreadedPlaySound(0);

        

        Sound[] soundFiles = new Sound[2]
        {

            new Sound(AppDomain.CurrentDomain.BaseDirectory + "175944__litruv__ghost-whispers.wav", 0.1f, 0.3f, false, false),
            new Sound(AppDomain.CurrentDomain.BaseDirectory + "252042__andy19__snake-pit.wav", 0.0f, 0.1f, true, true)
        };

        int soundCounter = 0;
        int deviceNumber = -1;
        for (int i = 0; i < WaveOut.DeviceCount; i++)
        {
            var cap = WaveOut.GetCapabilities(i);
            if (cap.ProductName.ToLower().Contains("headset") || 
                cap.ProductName.ToLower().Contains("speakers")
                    )
            {
                //if (deviceNumber == -1)
                    deviceNumber = i;
                if (soundCounter < 5)
                {
                    sl.Add(soundFiles[soundCounter]);
                    soundFiles[soundCounter].Init(deviceNumber);
                    
                }
                soundCounter++;
                if (soundCounter == soundFiles.Length) break;
            }
            
        }
        sl.Silence(true);
        soundFiles[0].Play();
        soundFiles[1].Play();


        //var device3 = playSound(1);
        for (int i = 0; i < 100; i++)
        {
            System.Threading.Thread.Sleep(100);
        }
        SerialPort btPort = new SerialPort();
        btPort.BaudRate = 9600;
        btPort.PortName = "COM3"; // Set in Windows
        int counter = 0;
        while (true)
        {
            counter++;
            try
            {
                btPort.Open();
                break;
            }
            catch(Exception ex)
            {
                if (counter == 10) throw;
                System.Threading.Thread.Sleep(500);
            }
        }

        string response = "";
        while (btPort.IsOpen)// && serialIncomingPort.IsOpen)
        {

            while (btPort.BytesToRead > 0)
            {
                while (btPort.BytesToRead > 0)
                {
                    response += Convert.ToChar(btPort.ReadChar());
                }
            }
            response = ParseResponse(response);
            btPort.Write("D"+dataDelimiter);
            
            // WRITE THE INCOMING BUFFER TO CONSOLE
            
            
            //Thread.Sleep(400);
            // SEND
        }
    }
}
