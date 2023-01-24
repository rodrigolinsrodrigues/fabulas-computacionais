using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionMananger : MonoBehaviour
{
    // como funfa?
    // 1. pra criar uma nova missao, deve a adicionar um int no statusMissao[] e um texto do txtMissao pelo inspector
        // 1.5. siga a numeração pros dois
    // 2. pra ativar uma missao, chame a variável do array statusMissao no script da ação e atribua o valor 1
    // 3. pra completar uma missao, chame a variável do array statusMissao no script da ação e atribua o valor 2

    // status das missões
    // 0 = não ativada | 1 = ativada | 2 = feita

    // status das missoes
    public int[] statusMissao;

    // textos das missoes
    public GameObject[] txtMissao;
    
    void Awake()
    {   // vai pegar os valores atuais das missoes que estao salvos no playerprefs e colocar no array int
        // ex.: o nome de uma variavel PlayerPrefs vai ser salva como "missao1"
        for (int i = 0; i < statusMissao.Length; i++)
        {
            statusMissao[i] = PlayerPrefs.GetInt("missao"+i.ToString()); 
        }
    }

    void Update()
    {   // vai ficar atualizando os valores dos status das missoes e textos
        for (int i = 0; i < statusMissao.Length; i++)
        {
            StatusMissao(statusMissao[i], txtMissao[i]);
        }
    }

    void StatusMissao(int _status, GameObject _txt) // atualiza os status das missoes e seus textos
    {
        switch (_status) 
        {           // missao
            case 0: // inativa
                _txt.SetActive(false);
                break;
            case 1: // ativa
                _txt.SetActive(true); // mostra o texto da missao
                break;
            case 2: // completada
                StartCoroutine(WaitToDisable(2f, _txt)); // vai marcar como feita e sumir
                break;
        }
    }

    IEnumerator WaitToDisable(float time, GameObject _txt) // vai desabilitar txt depois do cooldown
    {
        _txt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough; // marca como feita
        yield return new WaitForSeconds(time); // espera um cooldown
        _txt.SetActive(false); // desabilita o objeto
    }

    void OnDisable() // vai salvar os status das missoes nos playerprefs
    {
        for (int i = 0; i < statusMissao.Length; i++)
        {
            PlayerPrefs.SetInt("missao"+i.ToString(), statusMissao[i]);
        }
    }
}
