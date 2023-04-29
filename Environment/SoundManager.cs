using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Ascent.Environment
{
    public class SoundManager
    {
        private static SoundManager instance;

        private Dictionary<string, SoundEffectInstance> soundEffects;
        private Dictionary<string, Song> music;
        
        private SoundManager(ContentManager Content) { 
            soundEffects = new Dictionary<string, SoundEffectInstance>();
            soundEffects.Add("endLevel", Content.Load<SoundEffect>("Sounds/endLevel").CreateInstance());
            soundEffects.Add("gameStart", Content.Load<SoundEffect>("Sounds/gameStart").CreateInstance());
            soundEffects.Add("ropeFire", Content.Load<SoundEffect>("Sounds/ropeFire").CreateInstance());
            soundEffects.Add("hitSpikes", Content.Load<SoundEffect>("Sounds/hitSpikes").CreateInstance());
            soundEffects.Add("dash", Content.Load<SoundEffect>("Sounds/dash").CreateInstance());
            
            music = new Dictionary<string, Song>(); 
            music.Add("background", Content.Load<Song>("Sounds/backgroundMusic"));
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
            if (soundEffects.ContainsKey(soundName)){
                soundEffects[soundName].Play();
            }
        }

        public void PlayMusic(string songName)
        {
            MediaPlayer.Play(music[songName]);
            if (songName == "background")
            {
                MediaPlayer.IsRepeating = true;
            }
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
