﻿namespace Arcatech.Items
{
    public interface IWeapon : IUsableItem
    {
        public IDrawItemStrategy DrawStrategy { get; }
        

    }

   
}