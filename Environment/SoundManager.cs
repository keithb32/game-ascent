using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Ascent.Environment
{
    internal class SoundManager
    {
        private static SoundManager instance;

        private Dictionary<string, SoundEffectInstance> soundEffects;
        
        private SoundManager(ContentManager Content) { 
            soundEffects = new Dictionary<string, SoundEffectInstance>();
            soundEffects.Add("endLevel", Content.Load<SoundEffect>("Sounds/endLevel").CreateInstance());
        }

        public static SoundManager GetInstance(ContentManager Content)
        {
            if (instance == null)
            {
                instance = new SoundManager(Content);
            }
            return instance;
        }

        public void PlaySound(string soundName)
        {
            if (soundEffects.ContainsKey(soundName)){
                soundEffects[soundName].Play();
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
