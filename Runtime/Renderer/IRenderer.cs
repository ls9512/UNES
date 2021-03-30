namespace Aya.UNES.Renderer
{
    public interface IRenderer
    {
       string Name { get; }

       void HandleRender();

       void Init(UNESBehaviour nes);

       void End();
    }
}
