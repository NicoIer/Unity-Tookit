﻿using Nico.Design;

namespace Nico.Manager
{
    /// <summary>
    /// TODO 有待完成
    /// </summary>
    public class UIManager : GlobalSingleton<UIManager>
    {
        public void ShowUI<T>() where T : IUIPanel
        {
        }
    }
}