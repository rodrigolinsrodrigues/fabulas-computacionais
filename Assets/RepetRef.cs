using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepetRef : MonoBehaviour
{
    //Script serve como uma refencia para acerto nos botões de reétição, usado em casos especificos
    public checarPartitura CP;
    public int Referencia;
    // Start is called before the first frame update
    public void NotaP11(int Referencia)
    {
        CP.notap11 = Referencia;
    }

    public void notap12(int Referencia)
    {
        CP.notap12 = Referencia;
    }

     public void notap21(int Referencia)
    {
        CP.notap21 = Referencia;
    }
    
    public void notap22(int Referencia)
    {
        CP.notap22 = Referencia;
    }
}
