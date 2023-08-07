
// TODO: Funny thread language fix?
// TODO: Eventually, AM2E will just be a library instead of this weird dual-repository setup. At that point, this whole DesktopBootstrapper bit won't be part of the library; make sure it's removed then.

using var game = new AM2E.EngineCore(GameContent.EntryPoint.OnEntry);
game.Run();
