using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public interface IParticipanteRepository
    {
        Participante ObterParticipantePorId(Guid participanteId);
        void AdicionarParticipante(Participante participante);

    }
}
