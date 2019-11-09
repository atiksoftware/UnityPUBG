﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityPUBG.Scripts.Logic
{
    public class SandboxManager : MonoBehaviour
    {
        private void Start()
        {
            PhotonNetwork.offlineMode = true;

            RingSystem.Instance.GenerateRoundDatas();
            RingSystem.Instance.StartRingSystem();
        }
    }
}
