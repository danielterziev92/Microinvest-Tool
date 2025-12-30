using System;
using System.Drawing;

public static class IconGenerator
{
    public static Icon CreateAppIcon()
    {
        Bitmap bitmap = new Bitmap(64, 64);
        Graphics g = Graphics.FromImage(bitmap);
        
        // Background - blue circle
        g.Clear(Color.Transparent);
        SolidBrush bgBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
        g.FillEllipse(bgBrush, 0, 0, 64, 64);
        bgBrush.Dispose();
        
        // Draw worker person
        DrawWorker(g);
        
        // Draw wrench (tool)
        DrawWrench(g);
        
        // Border
        Pen borderPen = new Pen(Color.White, 3);
        g.DrawEllipse(borderPen, 2, 2, 60, 60);
        borderPen.Dispose();
        
        g.Dispose();
        
        IntPtr hIcon = bitmap.GetHicon();
        Icon icon = Icon.FromHandle(hIcon);
        
        return icon;
    }
    
    private static void DrawWorker(Graphics g)
    {
        // Head
        SolidBrush skinBrush = new SolidBrush(Color.FromArgb(255, 220, 180));
        g.FillEllipse(skinBrush, 22, 12, 20, 20);
        skinBrush.Dispose();
        
        // Hard hat
        SolidBrush hatBrush = new SolidBrush(Color.FromArgb(255, 200, 0));
        g.FillEllipse(hatBrush, 20, 8, 24, 12);
        g.FillRectangle(hatBrush, 18, 12, 28, 4);
        hatBrush.Dispose();
        
        // Body (shirt)
        SolidBrush shirtBrush = new SolidBrush(Color.FromArgb(255, 140, 0));
        Point[] bodyPoints = new Point[]
        {
            new Point(32, 32),  // neck
            new Point(18, 38),  // left shoulder
            new Point(18, 52),  // left bottom
            new Point(46, 52),  // right bottom
            new Point(46, 38),  // right shoulder
        };
        g.FillPolygon(shirtBrush, bodyPoints);
        shirtBrush.Dispose();
        
        // Arms
        SolidBrush armBrush = new SolidBrush(Color.FromArgb(255, 220, 180));
        g.FillRectangle(armBrush, 14, 36, 6, 16);  // Left arm
        g.FillRectangle(armBrush, 44, 36, 6, 16);  // Right arm
        armBrush.Dispose();
    }
    
    private static void DrawWrench(Graphics g)
    {
        // Wrench (gаечен ключ)
        Pen wrenchPen = new Pen(Color.White, 4);
        
        // Handle
        g.DrawLine(wrenchPen, 48, 40, 56, 48);
        
        // Head
        g.DrawLine(wrenchPen, 44, 36, 48, 40);
        g.DrawLine(wrenchPen, 48, 32, 52, 36);
        
        wrenchPen.Dispose();
        
        // Wrench head detail
        Pen detailPen = new Pen(Color.FromArgb(200, 200, 200), 2);
        g.DrawLine(detailPen, 46, 34, 50, 38);
        detailPen.Dispose();
    }
}