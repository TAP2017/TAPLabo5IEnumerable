using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Censor {
    public interface I{
        string Message { get; }
    }
    public static class CensorClass {
        public static IEnumerable<I> Censor(IEnumerable<I> sequence, string badWord){
            if (null==sequence)
                throw new ArgumentNullException(nameof(sequence), "cannot be null");
            return SafeCensor();

            IEnumerable<I> SafeCensor(){
                /* NON FUNZIONA
             1) perché se sequence è infinita e non contiene null Any non termina
             2)  perché in ogni caso dovrei fare due enumerazioni, quella dell'Any e quella in cui li uso
                e non ho nessuna garanzia che le due enumerazioni mi diano gli stessi risultati
                quindi è un controllo che non controlla veramente
             if (sequence.Any(x=>null==x)){
                throw new ArgumentNullException();
            }*/
                foreach (var x in sequence){
                    if (null == x){
                        throw new ArgumentNullException(nameof(sequence), "cannot contain a null element");
                    }

                    if (!new Regex(badWord).IsMatch(x.Message)){
                        yield return x;
                    }
                }
            }
        }
    }
}
