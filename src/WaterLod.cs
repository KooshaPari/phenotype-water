using System;

namespace Phenotype.Water
{
    /// <summary>
    /// Resolution tier returned by <see cref="WaterLod.SelectResolution"/>.
    /// </summary>
    public enum LodTier
    {
        /// <summary>Camera is within near distance — highest grid density.</summary>
        Near,

        /// <summary>Camera is within mid distance.</summary>
        Mid,

        /// <summary>Camera is within far distance — coarsest rendered grid.</summary>
        Far,

        /// <summary>Camera is beyond the cull distance — mesh should not be rendered.</summary>
        Culled,
    }

    /// <summary>
    /// Controls level-of-detail for the water surface mesh.
    /// Adjusts vertex density and wave evaluation frequency based on camera distance,
    /// ensuring consistent frame budget at all zoom levels.
    /// </summary>
    public class WaterLod
    {
        // ──────────────────────────────────────────────────────────────────────
        // Configurable thresholds
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Distance (in world units) at which the mesh transitions from
        /// <see cref="LodTier.Near"/> to <see cref="LodTier.Mid"/> quality.
        /// Default: 50.
        /// </summary>
        public float NearDistance { get; set; } = 50f;

        /// <summary>
        /// Distance (in world units) at which the mesh transitions from
        /// <see cref="LodTier.Mid"/> to <see cref="LodTier.Far"/> quality.
        /// Default: 150.
        /// </summary>
        public float MidDistance { get; set; } = 150f;

        /// <summary>
        /// Distance (in world units) at which the mesh transitions from
        /// <see cref="LodTier.Far"/> to <see cref="LodTier.Culled"/>.
        /// Geometry beyond this distance is not rendered.
        /// Default: 400.
        /// </summary>
        public float CullDistance { get; set; } = 400f;

        // ──────────────────────────────────────────────────────────────────────
        // Resolution per tier
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Grid resolution used for the <see cref="LodTier.Near"/> tier. Default: 64.</summary>
        public int NearResolution { get; set; } = 64;

        /// <summary>Grid resolution used for the <see cref="LodTier.Mid"/> tier. Default: 32.</summary>
        public int MidResolution { get; set; } = 32;

        /// <summary>Grid resolution used for the <see cref="LodTier.Far"/> tier. Default: 16.</summary>
        public int FarResolution { get; set; } = 16;

        // ──────────────────────────────────────────────────────────────────────
        // API
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the LOD tier appropriate for the given camera distance.
        /// </summary>
        /// <param name="distance">Camera-to-water distance in world units. Must be >= 0.</param>
        /// <returns>The <see cref="LodTier"/> for this distance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when distance is negative.</exception>
        public LodTier SelectTier(float distance)
        {
            if (distance < 0f)
                throw new ArgumentOutOfRangeException(nameof(distance), "distance must be >= 0");

            if (distance < NearDistance)   return LodTier.Near;
            if (distance < MidDistance)    return LodTier.Mid;
            if (distance < CullDistance)   return LodTier.Far;
            return LodTier.Culled;
        }

        /// <summary>
        /// Returns the grid resolution appropriate for the given camera distance.
        /// Returns 0 when the mesh should be culled.
        /// </summary>
        /// <param name="distance">Camera-to-water distance in world units. Must be >= 0.</param>
        /// <returns>
        /// One of <see cref="NearResolution"/>, <see cref="MidResolution"/>,
        /// <see cref="FarResolution"/>, or 0 if culled.
        /// </returns>
        public int SelectResolution(float distance)
        {
            return SelectTier(distance) switch
            {
                LodTier.Near   => NearResolution,
                LodTier.Mid    => MidResolution,
                LodTier.Far    => FarResolution,
                LodTier.Culled => 0,
                _              => 0,
            };
        }

        /// <summary>
        /// Validates that the configured thresholds are monotonically increasing:
        /// NearDistance &lt; MidDistance &lt; CullDistance.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if thresholds are not monotonically increasing.</exception>
        public void ValidateThresholds()
        {
            if (NearDistance >= MidDistance)
                throw new InvalidOperationException(
                    $"NearDistance ({NearDistance}) must be less than MidDistance ({MidDistance}).");
            if (MidDistance >= CullDistance)
                throw new InvalidOperationException(
                    $"MidDistance ({MidDistance}) must be less than CullDistance ({CullDistance}).");
        }
    }
}
