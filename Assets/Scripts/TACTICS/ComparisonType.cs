using UnityEngine;

namespace Conditions
{
    public enum ComparisonType
    {
        [InspectorName("<")]
        LesserThan,
        [InspectorName("<=")]
        LessOrEqualTo,
        [InspectorName("==")]
        EqualTo,
        [InspectorName(">=")]
        GreaterOrEqualTo,
        [InspectorName(">")]
        GreaterThan,
        [InspectorName("!=")]
        NotEqualTo
    }
}