﻿using TAlex.BeautifulFractals.Rendering;


namespace TAlex.BeautifulFractals.Fractals
{
    public abstract class Fractal3D : Fractal
    {
        public abstract void Render(IGraphics3DContext context);
    }
}
