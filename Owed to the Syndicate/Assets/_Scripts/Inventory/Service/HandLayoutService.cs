using UnityEngine;

/// <summary>
/// Service: Handles fan-based card layout calculations for hand positioning.
/// Provides positions and rotations for cards arranged in a fan pattern.
/// </summary>
public class HandLayoutService
{
    private readonly float cardWidth;
    private readonly float maxRotationAngle;
    private readonly float verticalCurveDepth;

    /// <summary>
    /// Creates a new HandLayoutService with specified layout parameters.
    /// </summary>
    /// <param name="cardWidth">Width of each card for spacing calculations</param>
    /// <param name="maxRotationAngle">Maximum rotation angle in degrees for outermost cards</param>
    /// <param name="verticalCurveDepth">Depth of the vertical curve for the fan effect</param>
    public HandLayoutService(float cardWidth = 120f, float maxRotationAngle = 15f, float verticalCurveDepth = 30f)
    {
        this.cardWidth = cardWidth;
        this.maxRotationAngle = maxRotationAngle;
        this.verticalCurveDepth = verticalCurveDepth;
    }

    /// <summary>
    /// Calculates the position and rotation for a card at a specific index in the hand.
    /// Cards are arranged in a fan pattern with rotation and vertical curve.
    /// </summary>
    /// <param name="index">Zero-based index of the card in the hand</param>
    /// <param name="totalCards">Total number of cards in the hand</param>
    /// <returns>Local position and rotation for the card</returns>
    public (Vector3 position, Quaternion rotation) GetFanTransform(int index, int totalCards)
    {
        if (totalCards <= 0)
        {
            return (Vector3.zero, Quaternion.identity);
        }

        if (totalCards == 1)
        {
            return (Vector3.zero, Quaternion.identity);
        }

        // Calculate normalized position (-1 to 1, where 0 is center)
        float normalizedIndex = (totalCards > 1)
            ? (index / (float)(totalCards - 1)) * 2f - 1f
            : 0f;

        // Horizontal spacing
        float horizontalSpacing = CalculateHorizontalSpacing(totalCards);
        float xPosition = normalizedIndex * horizontalSpacing;

        // Vertical curve (cards at edges dip down)
        float yPosition = -Mathf.Abs(normalizedIndex) * verticalCurveDepth;

        // Rotation (cards at edges rotate outward)
        float rotationAngle = normalizedIndex * maxRotationAngle;

        Vector3 position = new Vector3(xPosition, yPosition, 0f);
        Quaternion rotation = Quaternion.Euler(0f, 0f, -rotationAngle);

        return (position, rotation);
    }

    /// <summary>
    /// Calculates horizontal spacing between cards based on hand size.
    /// Reduces spacing when hand is large to prevent overflow.
    /// </summary>
    private float CalculateHorizontalSpacing(int totalCards)
    {
        if (totalCards <= 1)
            return 0f;

        // Base spacing is card width * 0.7 (70% overlap for nice fan effect)
        float baseSpacing = cardWidth * 0.7f;

        // Maximum total width before we start compressing
        float maxHandWidth = 800f;
        float totalWidth = baseSpacing * (totalCards - 1);

        if (totalWidth > maxHandWidth)
        {
            // Compress spacing to fit within max width
            baseSpacing = maxHandWidth / (totalCards - 1);
        }

        return baseSpacing * 0.5f; // Half because we use normalized index from -1 to 1
    }

    /// <summary>
    /// Gets the total width the hand will occupy.
    /// Useful for centering or collision detection.
    /// </summary>
    public float GetTotalHandWidth(int totalCards)
    {
        if (totalCards <= 1)
            return cardWidth;

        float spacing = CalculateHorizontalSpacing(totalCards);
        return spacing * 2f * (totalCards - 1) + cardWidth;
    }

    /// <summary>
    /// Gets all transforms for a full hand at once.
    /// Useful for batch layout operations.
    /// </summary>
    public (Vector3 position, Quaternion rotation)[] GetAllFanTransforms(int totalCards)
    {
        var transforms = new (Vector3, Quaternion)[totalCards];

        for (int i = 0; i < totalCards; i++)
        {
            transforms[i] = GetFanTransform(i, totalCards);
        }

        return transforms;
    }
}