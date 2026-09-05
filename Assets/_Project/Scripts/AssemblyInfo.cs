using System.Runtime.CompilerServices;

// Editor tooling (project bootstrap / prefab generation) and tests may touch internal serialized fields.
[assembly: InternalsVisibleTo("Starfall.Editor")]
[assembly: InternalsVisibleTo("Starfall.Tests.EditMode")]
[assembly: InternalsVisibleTo("Starfall.Tests.PlayMode")]
