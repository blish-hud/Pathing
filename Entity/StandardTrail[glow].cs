using System;
using System.Collections.Generic;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity;

#nullable enable
public partial class StandardTrail {
    public const float MaxGlowLength = 1.5f;
    

    private float? _distance = null;
    public float Distance {
        get {
            if (_distance is not null) return _distance.Value;

            float    distance = 0;
            foreach (Vector3[] section in _sectionPoints) {
                for (int i = 0; i < section.Length - 1; i++) {
                    distance += Vector3.Distance(section[i], section[i + 1]);
                }
            }
            _distance = distance;
            return distance;
        }
    }
    
    private Vector3[] GenerateGlowPoints(float resolution) {
        var points = new List<Vector3>();
        points.Add(_sectionPoints[0][0]);
        foreach (Vector3[] section in _sectionPoints) {
            for (int i = 0; i < section.Length - 1; i++) {
                var   current  = section[i];
                var   next     = section[i + 1];
                float distance = Vector3.Distance(current, next);
                if (distance <= resolution) {
                    points.Add(next);
                    continue;
                }
                // Split segment
                var segment = (current - next) / distance;
                int splits = (int) Math.Floor(distance / resolution);
                segment *= resolution;
                for (int j = 0; j < splits; j++) {
                    points.Add(current -= segment);
                }
                points.Add(next);
            }
        }

        return points.ToArray();
    }

    private float?     _lastGlowResolution  = null;
    private int?       _lastGlowBeadSpacing = null;
    private int        _glowBeadCount       = 0;
    private Vector3[]? _glowPoints          = null;
    
    private float _glowSpeed       => _packState.UserConfiguration.MapTrailGlowSpeed.Value;
    private int   _glowBeadSpacing => _packState.UserConfiguration.MapTrailGlowBeadSpacing.Value;
    
     private void RenderGlow(SpriteBatch spriteBatch, Rectangle bounds, double offsetX, double offsetY, double scale, bool mapOpen) {
         
         if (_glowPoints is null || _lastGlowResolution != MaxGlowLength) {
             this._lastGlowResolution  = MaxGlowLength;
             this._glowPoints          = GenerateGlowPoints(MaxGlowLength);
             this._lastGlowBeadSpacing = null; // regenerate bead spacing
         }
         if (_lastGlowBeadSpacing == null || _lastGlowBeadSpacing != this._glowBeadSpacing) {
             this._lastGlowBeadSpacing = this._glowBeadSpacing;
             this._glowBeadCount       = this._glowPoints.Length / this._glowBeadSpacing;
         }
         
         int globalFrame = (int) Math.Floor((GameService.Overlay.CurrentGameTime.TotalGameTime.TotalSeconds * _glowSpeed) % this._glowBeadSpacing);

         for (int i = 0; i < _glowBeadCount; i++) {
             int relativeFrame = (i * this._glowBeadSpacing) + globalFrame;
             try {
                 RenderGlowBead(spriteBatch, bounds, offsetX, offsetY, scale, relativeFrame, mapOpen);
             } catch (IndexOutOfRangeException) {
                 break; // since relativeFrame will increase with each iteration, if it goes over we know all future iterations will go over too.
             }
         }
     }

     private float _glowBeadMapOpacity     = 1f;
     private float _glowBeadMinimapOpacity = 1f;
     
     private void RenderGlowBead(SpriteBatch spriteBatch, Rectangle bounds, double offsetX, double offsetY, double scale, int frame, bool mapOpen) {
         var current = _glowPoints![frame];
         var next    = _glowPoints![frame + 1];
         var currentScaled  = GetScaledLocation(current.X, current.Y,     scale, offsetX, offsetY);
         var nextScaled  = GetScaledLocation(next.X, next.Y, scale, offsetX, offsetY);

         if (!bounds.Contains(currentScaled) && !bounds.Contains(nextScaled)) {
             return;
         }
         if (Vector3.Distance(current, next) > MaxGlowLength + 1) {
             return;
         }

         float distance    = Vector2.Distance(currentScaled, nextScaled);
         float angle       = (float) Math.Atan2(nextScaled.Y - currentScaled.Y, nextScaled.X - currentScaled.X);
         float opacity     = HeightCorrectOpacity(current, next, mapOpen ? _glowBeadMapOpacity : _glowBeadMinimapOpacity);

         DrawLine(spriteBatch, currentScaled, angle, distance, _glowBeadColor * opacity, _packState.UserConfiguration.MapTrailWidth.Value);
     }

     private void UpdateMapGlowOpacity(float newMapOpacity) {
         _glowBeadMapOpacity = MathHelper.Clamp(newMapOpacity * _packState.UserResourceStates.Advanced.GlowTrailOpacityPercent, 0f, 1f);
     }
     
     private void UpdateMiniMapGlowOpacity(float newMiniMapOpacity) {
         _glowBeadMinimapOpacity = MathHelper.Clamp(newMiniMapOpacity * _packState.UserResourceStates.Advanced.GlowTrailOpacityPercent, 0f, 1f);
     }

}
