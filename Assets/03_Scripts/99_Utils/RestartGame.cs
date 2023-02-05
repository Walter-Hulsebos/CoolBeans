using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace CoolBeans.Utils
{
    public sealed class RestartGame : MonoBehaviour
    {
        
        
        [PublicAPI]
        public void Restart()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
        }
    }
}
