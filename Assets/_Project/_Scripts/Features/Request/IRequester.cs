using System.Collections.Generic;

namespace PaintFlow.Features.Requester
{
    public interface IRequester
    {
        int Id { get; }
        float PositionT { get; }
        ColorRequest CurrentRequest { get; }
        bool IsCompleted { get; }

        void Initialize(int id, float positionT, List<ColorRequest> requests);
    }
}