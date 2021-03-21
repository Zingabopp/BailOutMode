using SiraUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BailOutMode
{
    internal class BailOutInstaller : Installer
    {
        public override void InstallBindings()
        {
            Plugin.Log?.Info("Injecting Dependencies");
            Container.Bind<BailOutController>().FromNewComponentOnNewGameObject("BailOutController").AsSingle().NonLazy();
        }
    }
}
