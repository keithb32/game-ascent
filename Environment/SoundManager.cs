using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Ascent.Environment
{
    public class SoundManager
    {
        private static SoundManager instance;

        private Dictionary<string, SoundEffectInstance> soundEffects;
        private Dictionary<string, SoundEffectInstance> music;
        private SoundEffectInstance currentSong;
        
        private SoundManager(ContentManager Content) { 
            soundEffects = new Dictionary<string, SoundEffectInstance>();
            soundEffects.Add("endLevel", Content.Load<SoundEffect>("Sounds/endLevel").CreateInstance());
            soundEffects.Add("gameStart", Content.Load<SoundEffect>("Sounds/gameStart").CreateInstance());
            soundEffects.Add("ropeFire", Content.Load<SoundEffect>("Sounds/ropeFire").CreateInstance());
            soundEffects.Add("hitSpikes", Content.Load<SoundEffect>("Sounds/hitSpikes").CreateInstance());
            soundEffects.Add("dash", Content.Load<SoundEffect>("Sounds/dash").CreateInstance());
            
            music = new Dictionary<string, SoundEffectInstance>(); 
            music.Add("Title", Content.Load<SoundEffect>("Sounds/Music/Title").CreateInstance());
            music.Add("Level1", Content.Load<SoundEffect>("Sounds/Music/Level1").CreateInstance());
            music.Add("Level2", Content.Load<SoundEffect>("Sounds/Music/Level2").CreateInstance());
            music.Add("Level3", Content.Load<SoundEffect>("Sounds/Music/Level3").CreateInstance());
            music.Add("Level4", Content.Load<SoundEffect>("Sounds/Music/Level4").CreateInstance());
        }

        public static SoundManager CreateInstance(ContentManager Content)
        {
            if (instance == null)
            {
                instance = new SoundManager(Content);
            }
            return instance;
        }
        public static SoundManager GetInstance()
        {
            return instance;
        }

        public void PlaySound(string soundName)
        {
            if (soundEffects.TryGetValue(soundName, out var effect)){
                effect.Play();
            }
        }

        public void PlayMusic(string songName)
        {

            currentSong?.Stop();
            music[songName].IsLooped = true;
            music[songName].Play();
            currentSong = music[songName];
        }

        public bool IsPlaying(string soundName)
        {
            if (soundEffects.ContainsKey(soundName))
            {
                return soundEffects[soundName].State == SoundState.Playing;
            }
            return false;
        }

    }
}
