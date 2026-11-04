using System;
using System.Collections.Generic;
using System.Text;

namespace RaizesNordeste.Domain.Enums
{
    public enum StatusPedido
    {
        AGUARDANDO_PAGAMENTO,
        RECEBIDO,
        EM_PREPARO,
        PRONTO,
        ENTREGUE,
        CANCELADO,
        PAGAMENTO_RECUSADO
    }
}
