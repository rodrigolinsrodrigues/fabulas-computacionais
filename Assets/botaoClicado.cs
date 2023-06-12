using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class botaoClicado : MonoBehaviour
{
    private Button button;
    public  bool buttonClicked = false;

    private void Start()
    {
        // Obtenha a referência para o botão
        button = GetComponent<Button>();

        // Adicione um ouvinte para o evento de clique do botão
        button.onClick.AddListener(ButtonClick);
    }

    private void Update()
    {
        // Verifique se o botão não foi clicado
        if (!buttonClicked)
        {
            Debug.Log("O botão não foi clicado.");
            // Execute o código desejado aqui
        }
    }

    private void ButtonClick()
    {
        // Defina a variável buttonClicked como true quando o botão for clicado
        buttonClicked = true;
        Debug.Log("O botão foi clicado.");
    }
}
