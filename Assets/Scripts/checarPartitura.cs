using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class checarPartitura : MonoBehaviour
{
    public Play play;
    public RepeatPart2 play2;
    public GameObject[] NotaPart1, NotaPart2; //Notas corretas para a aprovação no minigame
    public GameObject[] RepetPart1, RepetPart2; // Seletores de repetição de cada partitura
    public string[] TipoNotaPart1, TipoNotaPart2; // Tipos de sons atrbuidos as notas
    public int notap11, notap12, notap21, notap22;// Serve para controlar o intervalo de repetições em ambas as partituras
    public GameObject[] BloqPart1, Bloqpart2;
    public GameObject[] BloqRepeatPart1, BloqRepeatPart2;
    public GameObject ContadorP1, ContadorP2; // Identifica o contador de repetição selecionado  em cada partitura
    public GameObject ContP1, ContP2; // Serve para fazer a checagem do identificador no script
    public bool check, part1, part1repet, part2, part2repet, ApenasPart1, Fase4, Fase2, Fase3, acabou; // Bools de checagem a depender do caso
    public GameObject conluido, tenteDnv; // Pega o gameobjc do canvas 
    public GameObject botãoClicado1Part1, botãoClicado2Part1, botãoClicado1Part2, botãoClicado2Part2; // identifica os botões clicados vinculados as repetições de cada partitura

    public GameObject gameHolder, playerObj, gamePlayer;
    public Animator redomeAnimator;

    public LevelDataManager dataManager;

    void Start()
    {
        dataManager = FindObjectOfType<LevelDataManager>();
    }

    public void Update()
    {

    }

    public void Check()
    {
        if ((part1 == false && part1repet == false) || (part2 == false && part2repet == false))
        {
            play.qtdRepete = play.quantidadeSelecionada;
            if (play2 != null)
            {
                play2.qtdRepete = play2.quantidadeSelecionada;
            }
        }
        //StartCoroutine("Delay");

        if (ApenasPart1 == true)
        {
            if (RepetPart1[0] != null && RepetPart1[1] != null && ContP1 != null)
            {
                Partitura1Repet();
            }
            else
            {
                Partitura1();
            }

            //StartCoroutine("Delay2");
            LogSolution(part1 == true || part1repet == true);
            if (part1 == true || part1repet == true)
            {
                EventSystem.current.SetSelectedGameObject(null);
                conluido.SetActive(true);
                //Codigo para computar vitória
                GameManager.Instance.HideAllControlPopup();
                GameManager.Instance.UpdateProgress(SceneManager.GetActiveScene().name);
                dataManager.WinLevel();
                redomeAnimator.SetBool("abrir", true);
                StartCoroutine("EndGame");
            }
            else
            {
                check = false;
            }
        }
        else
        {

            if (RepetPart1[0] != null && RepetPart1[1] != null && ContP1 != null)
            {
                Partitura1Repet();
            }
            else
            {
                Partitura1();
            }


            if (RepetPart2[0] != null && RepetPart2[1] != null && ContP2 != null)
            {
                Partitura2Repet();
            }
            else
            {
                Partitura2();
            }

            //StartCoroutine("Delay2");
            LogSolution((part1 == true || part1repet == true) && (part2 == true || part2repet == true));
            if ((part1 == true || part1repet == true) && (part2 == true || part2repet == true))
            {
                EventSystem.current.SetSelectedGameObject(null);
                conluido.SetActive(true);
                //Codigo para computar vitória
                GameManager.Instance.HideAllControlPopup();
                GameManager.Instance.UpdateProgress(SceneManager.GetActiveScene().name);
                dataManager.WinLevel();
                redomeAnimator.SetBool("abrir", true);
                StartCoroutine("EndGame");
            }
            else
            {
                check = false;
            }

        }
    }

    void Partitura1Repet()
    {
        Debug.Log("Repet funcionando");
        //Repetição
        if (Fase2 == true)
        {
            if ((NotaPart1[notap11].GetComponent<AudioSource>().clip.name != TipoNotaPart1[notap11]
            || NotaPart1[notap12].GetComponent<AudioSource>().clip.name != TipoNotaPart1[notap12] ||
            BloqPart1[0].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[1].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[2].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[3].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[4].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[5].GetComponent<botaoClicado>().buttonClicked == true))
            {
                part1repet = false;
            }
            else if ((NotaPart1[notap11].GetComponent<AudioSource>().clip.name == TipoNotaPart1[notap11] &&
            NotaPart1[notap12].GetComponent<AudioSource>().clip.name == TipoNotaPart1[notap12]) &&
            (RepetPart1[0] == botãoClicado1Part1 && RepetPart1[1] == botãoClicado2Part1 && ContP1 == ContadorP1))
            {
                part1repet = true;
            }
        }

        if (Fase3 == true)
        {
            if ((NotaPart1[notap11].GetComponent<AudioSource>().clip.name != TipoNotaPart1[notap11]
            || NotaPart1[notap12].GetComponent<AudioSource>().clip.name != TipoNotaPart1[notap12] ||
            BloqPart1[0].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[1].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[2].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[3].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[4].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[5].GetComponent<botaoClicado>().buttonClicked == true))
            {
                part1repet = false;
            }
            else if ((NotaPart1[notap11].GetComponent<AudioSource>().clip.name == TipoNotaPart1[notap11] &&
            NotaPart1[notap12].GetComponent<AudioSource>().clip.name == TipoNotaPart1[notap12]) &&
            (RepetPart1[0] == botãoClicado1Part1 && RepetPart1[1] == botãoClicado2Part1 && ContP1 == ContadorP1))
            {
                part1repet = true;
            }
        }

        if (Fase4 == true)
        {
            if ((NotaPart1[notap11].GetComponent<AudioSource>().clip.name != TipoNotaPart1[notap11] ||
            BloqPart1[0].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[1].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[2].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[3].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[4].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[5].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[6].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[7].GetComponent<botaoClicado>().buttonClicked == true ||
            BloqPart1[8].GetComponent<botaoClicado>().buttonClicked == true))
            {
                part1repet = false;
            }
            else if ((NotaPart1[notap11].GetComponent<AudioSource>().clip.name == TipoNotaPart1[notap11]) &&
            (RepetPart1[0] == botãoClicado1Part1 && RepetPart1[1] == botãoClicado2Part1 && ContP1 == ContadorP1))
            {
                part1repet = true;
            }
        }




    }
    void Partitura1()
    {
        if (NotaPart1[0].GetComponent<AudioSource>().clip.name != null && NotaPart1[1].GetComponent<AudioSource>().clip.name != null && NotaPart1[2].GetComponent<AudioSource>().clip.name != null && NotaPart1[3].GetComponent<AudioSource>().clip.name != null)
        {
            if (NotaPart1[0].GetComponent<AudioSource>().clip.name != TipoNotaPart1[0] || NotaPart1[1].GetComponent<AudioSource>().clip.name != TipoNotaPart1[1]
            || NotaPart1[2].GetComponent<AudioSource>().clip.name != TipoNotaPart1[2] || NotaPart1[3].GetComponent<AudioSource>().clip.name != TipoNotaPart1[3])
            {
                part1 = false;
            }

            else if (NotaPart1[0].GetComponent<AudioSource>().clip.name == TipoNotaPart1[0] && NotaPart1[1].GetComponent<AudioSource>().clip.name == TipoNotaPart1[1]
            && NotaPart1[2].GetComponent<AudioSource>().clip.name == TipoNotaPart1[2] && NotaPart1[3].GetComponent<AudioSource>().clip.name == TipoNotaPart1[3])
            {

                part1 = true;

            }
        }


    }

    void Partitura2Repet()
    {
        if (Fase3 == true)
        {
            if ((NotaPart2[notap21].GetComponent<AudioSource>().clip.name != TipoNotaPart2[notap21] ||
            NotaPart2[notap22].GetComponent<AudioSource>().clip.name != TipoNotaPart2[notap22]) ||
            Bloqpart2[0].GetComponent<botaoClicado>().buttonClicked == true ||
            Bloqpart2[1].GetComponent<botaoClicado>().buttonClicked == true ||
            Bloqpart2[2].GetComponent<botaoClicado>().buttonClicked == true)
            {
                part2repet = false;
            }

            else if ((NotaPart2[notap21].GetComponent<AudioSource>().clip.name == TipoNotaPart2[notap21] &&
            NotaPart2[notap22].GetComponent<AudioSource>().clip.name == TipoNotaPart2[notap22]) &&
            (RepetPart2[0] == botãoClicado1Part2 && RepetPart2[1] == botãoClicado2Part2 && ContP2 == ContadorP2))
            {
                part2repet = true;
            }
        }

        if (Fase4 == true)
        {
            if (NotaPart2[notap21].GetComponent<AudioSource>().clip.name != TipoNotaPart2[notap21] ||
            Bloqpart2[0].GetComponent<botaoClicado>().buttonClicked == true ||
            Bloqpart2[1].GetComponent<botaoClicado>().buttonClicked == true ||
            Bloqpart2[2].GetComponent<botaoClicado>().buttonClicked == true)
            {
                part2repet = false;
            }

            else if ((NotaPart2[notap21].GetComponent<AudioSource>().clip.name == TipoNotaPart2[notap21] &&
            NotaPart2[2].GetComponent<botaoClicado>().buttonClicked == true &&
            NotaPart2[3].GetComponent<botaoClicado>().buttonClicked == true) &&
            (RepetPart2[0] == botãoClicado1Part2 && RepetPart2[1] == botãoClicado2Part2 && ContP2 == ContadorP2))
            {
                part2repet = true;
            }
        }



    }
    void Partitura2()
    {
        if (Fase3 == true)
        {
            if (NotaPart2[0].GetComponent<AudioSource>().clip != null && NotaPart2[1].GetComponent<AudioSource>().clip != null && NotaPart2[2].GetComponent<AudioSource>().clip != null && NotaPart2[3].GetComponent<AudioSource>().clip != null)
            {
                if (NotaPart2[0].GetComponent<AudioSource>().clip.name != TipoNotaPart2[0] || NotaPart2[1].GetComponent<AudioSource>().clip.name != TipoNotaPart2[1]
                || NotaPart2[2].GetComponent<AudioSource>().clip.name != TipoNotaPart2[2] || NotaPart2[3].GetComponent<AudioSource>().clip.name != TipoNotaPart2[3])
                {
                    part2 = false;
                }

                if (NotaPart2[0].GetComponent<AudioSource>().clip.name == TipoNotaPart2[0] && NotaPart2[1].GetComponent<AudioSource>().clip.name == TipoNotaPart2[1]
                && NotaPart2[2].GetComponent<AudioSource>().clip.name == TipoNotaPart2[2] && NotaPart2[3].GetComponent<AudioSource>().clip.name == TipoNotaPart2[3])
                {
                    part2 = true;
                }

            }
        }

        if (Fase4 == true)
        {
            //normal
            if (NotaPart2[0].GetComponent<AudioSource>().clip != null && NotaPart2[1].GetComponent<AudioSource>().clip != null && NotaPart2[2].GetComponent<AudioSource>().clip != null && NotaPart2[3].GetComponent<AudioSource>().clip != null)
            {
                if (NotaPart2[0].GetComponent<AudioSource>().clip.name == TipoNotaPart2[0] && NotaPart2[1].GetComponent<AudioSource>().clip.name == TipoNotaPart2[1]
                && NotaPart2[2].GetComponent<AudioSource>().clip.name == TipoNotaPart2[2] && NotaPart2[3].GetComponent<AudioSource>().clip.name == TipoNotaPart2[3])
                {
                    part2 = true;
                }
                if (NotaPart2[0].GetComponent<AudioSource>().clip.name != TipoNotaPart2[0] || NotaPart2[1].GetComponent<AudioSource>().clip.name != TipoNotaPart2[1]
                || NotaPart2[2].GetComponent<AudioSource>().clip.name != TipoNotaPart2[2] || NotaPart2[3].GetComponent<AudioSource>().clip.name != TipoNotaPart2[3])
                {
                    part2 = false;
                }
            }
        }

    }

    // Learning analytics: estado da partitura no momento da verificação.
    void LogSolution(bool success)
    {
        var slots = new List<string>();
        var expected = new List<string>();
        AddPart(NotaPart1, TipoNotaPart1, slots, expected);
        bool hasPart2 = !ApenasPart1 && NotaPart2 != null;
        if (hasPart2) AddPart(NotaPart2, TipoNotaPart2, slots, expected);

        SolutionTracker.LogAttempt("Musical", slots.ToArray(), expected.ToArray(), success,
            ("phase_flags", (Fase2 ? "F2 " : "") + (Fase3 ? "F3 " : "") + (Fase4 ? "F4" : "")),
            ("part1_notes", part1), ("part1_repeat", part1repet),
            ("part2_notes", part2), ("part2_repeat", part2repet),
            ("part1_size", NotaPart1 != null ? NotaPart1.Length : 0),
            ("p1_repeat_start", SolutionTracker.NameOf(RepetPart1 != null && RepetPart1.Length > 0 ? RepetPart1[0] : null)),
            ("p1_repeat_end", SolutionTracker.NameOf(RepetPart1 != null && RepetPart1.Length > 1 ? RepetPart1[1] : null)),
            ("p1_repeat_counter", SolutionTracker.NameOf(ContP1)),
            ("p1_repeat_expected_slots", new[] { notap11, notap12 }),
            ("p1_blocked_slots_used", CountClicked(BloqPart1)),
            ("p2_repeat_start", SolutionTracker.NameOf(RepetPart2 != null && RepetPart2.Length > 0 ? RepetPart2[0] : null)),
            ("p2_repeat_end", SolutionTracker.NameOf(RepetPart2 != null && RepetPart2.Length > 1 ? RepetPart2[1] : null)),
            ("p2_repeat_counter", SolutionTracker.NameOf(ContP2)),
            ("p2_repeat_expected_slots", new[] { notap21, notap22 }),
            ("p2_blocked_slots_used", CountClicked(Bloqpart2)),
            ("repeat_times_selected", play != null ? play.quantidadeSelecionada : 0));
    }

    static void AddPart(GameObject[] notes, string[] types, List<string> slots, List<string> expected)
    {
        if (notes == null) return;
        for (int i = 0; i < notes.Length; i++)
        {
            slots.Add(SolutionTracker.ClipName(notes[i]));
            expected.Add(types != null && i < types.Length ? types[i] : "");
        }
    }

    static int CountClicked(GameObject[] buttons)
    {
        int n = 0;
        if (buttons == null) return 0;
        foreach (var b in buttons)
        {
            var bc = b != null ? b.GetComponent<botaoClicado>() : null;
            if (bc != null && bc.buttonClicked) n++;
        }
        return n;
    }

    IEnumerator EndGame()
    {
        EventSystem.current.SetSelectedGameObject(null);

        yield return new WaitForSeconds(2);
        playerObj.SetActive(true);
        gameHolder.SetActive(false);
        gamePlayer.SetActive(false);
        GameManager.Instance.canPause = true;
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(2);
        check = false;

    }
    IEnumerator Delay2()
    {
        yield return new WaitForSeconds(1);
        check = false;
    }
    public void repet1Part1(GameObject botãoClicado)
    {
        RepetPart1[0] = botãoClicado;
        botãoClicado1Part1 = botãoClicado;
    }

    public void repet2Part1(GameObject botãoClicado)
    {
        RepetPart1[1] = botãoClicado;
        botãoClicado2Part1 = botãoClicado;
    }

    public void repet1Part2(GameObject botãoClicado)
    {
        RepetPart2[0] = botãoClicado;
        botãoClicado1Part2 = botãoClicado;
    }

    public void repet2Part2(GameObject botãoClicado)
    {
        RepetPart2[1] = botãoClicado;
        botãoClicado2Part2 = botãoClicado;
    }

    public void ContadorRepetição(GameObject ContadorSelecionado)
    {
        ContadorP1 = ContadorSelecionado;
        ContP1 = ContadorP1;

    }
    public void ContadorRepetição2(GameObject ContadorSelecionado)
    {
        ContadorP2 = ContadorSelecionado;
        ContP2 = ContadorP2;
    }


}

