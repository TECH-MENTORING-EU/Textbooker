namespace Booker.E2e;

/// <summary>
/// One app + one database + one browser for the whole (serialized) assembly:
/// the journeys share state by design (the seeded item, favorites toggles),
/// and a single Kestrel host pair removes boot-order races between classes.
/// </summary>
[CollectionDefinition("E2E")]
public sealed class E2eCollection : ICollectionFixture<E2eWebAppFixture>;
