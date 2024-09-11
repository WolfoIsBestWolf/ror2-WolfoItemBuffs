using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace WolfoItemBuffs
{
	public class WRect
	{
        public static readonly System.Random random = new System.Random();

        public static Rect rec128 = new Rect(0, 0, 128, 128);
        public static Vector2 half = new Vector2(0.5f, 0.5f);
    }
}