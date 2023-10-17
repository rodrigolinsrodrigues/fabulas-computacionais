using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class LerMesa : MonoBehaviour
{
    public Snap[] foodTables, actionTables;
    public GameObject[] finalFood;
    public GameObject errorFood;
    public GameObject[] finalPlates;
    public Transform[] finalFoodPosition;
    public Transform[] playerPositions;
    public Transform prepareTablePos, finalTablePos;
    public GameObject[] cams;
    public bool comecar, parar;
    public GameObject player, hand;
    public ListaDeRecursos foodBank; // Acessa o banco de modelos de insumos
    private bool missed = false;
    public GameObject startButton, winPanel, jewelObj;
    public Vector3 playerRotation;
    public Animator cookingObjAnimator;
    public GameObject smoke;
    public LevelDataManager dataManager;
    private void Start()
    {
        playerRotation = player.transform.eulerAngles;
        dataManager = FindObjectOfType<LevelDataManager>();
    }
    public void iniciar()
    {
        comecar = true;
        parar = false;
        if (comecar == true && parar == false)
        {
            GameManager.Instance.canPause = false;
            StartCoroutine("TempoDeEspera");
            //player.GetComponent<PlayerController>().ResetAnimation();
            player.GetComponent<PlayerController>().enabled = false;
            hand.GetComponent<PickAndDrop>().enabled = false;
            cams[0].SetActive(false);
            cams[1].SetActive(true);
            ResetFinalFood();
        }

    }
    public IEnumerator TempoDeEspera()
    {
        dataManager.GetAttempt();
        player.transform.eulerAngles = playerRotation;
        for (int i = 0; i < foodTables.Length; i++)
        {
            if (!foodTables[i].isOccupied || !actionTables[i].isOccupied)
            {
                yield return new WaitForSeconds(1);
                missed = true;
                break;
            }
            else
            {
                //teleporta o player pra mesa atual
                player.transform.position = playerPositions[i].position;

                // verifica que tipo de interação vai ser feita
                switch (actionTables[i].currentType)
                {
                    //Cortar
                    case "cut":
                        player.GetComponent<PlayerController>().animator.SetBool("Fritar", false);
                        player.GetComponent<PlayerController>().animator.SetBool("Cortar", true);
                        yield return new WaitForSeconds(0.5f);
                        GameObject smokeClone = Instantiate(smoke, foodTables[i].objPosition.transform.position, Quaternion.identity);
                        Destroy(smokeClone, 1);
                        yield return new WaitForSeconds(0.5f);
                        ChangeState(foodTables[i].obj.GetComponent<Item>(), 1);
                        break;
                    //Fritar
                    case "fry":
                        player.GetComponent<PlayerController>().animator.SetBool("Fritar", true);
                        player.GetComponent<PlayerController>().animator.SetBool("Cortar", false);
                        yield return new WaitForSeconds(0.5f);
                        GameObject smokeClone2 = Instantiate(smoke, foodTables[i].objPosition.transform.position, Quaternion.identity);
                        Destroy(smokeClone2, 1);
                        yield return new WaitForSeconds(0.5f);
                        ChangeState(foodTables[i].obj.GetComponent<Item>(), 2);
                        break;
                }

                yield return new WaitForSeconds(2);
                //reseta a animação para parado
                player.GetComponent<PlayerController>().ResetAnimation();

                yield return new WaitForSeconds(0.5f);


            }
        }
        if (!missed)
        {
            //faz animação de preparo e posiciona o player para assistir
            player.transform.position = prepareTablePos.position;
            cookingObjAnimator.SetBool("cooking", true);
            yield return new WaitForSeconds(2.2f);
            //posiciona o player para ver o resultado final
            player.transform.position = finalTablePos.position;
            cookingObjAnimator.SetBool("cooking", false);
            yield return new WaitForSeconds(0.2f);
            CheckFinal();
        }
        else
        {
            EndOfRead();
        }
    }

    //muda o estado da comida (visual)
    public void ChangeState(Item food, int state)
    {
        food.meshFilter.sharedMesh = foodBank.SelectFood(food.type, state).GetComponent<MeshFilter>().sharedMesh;
    }

    //Verifica se o jogador acertou tudo
    public void CheckFinal()
    {

        for (int i = 0; i < foodTables.Length; i++)
        {
            if (!foodTables[i].isCorrect || !actionTables[i].isCorrect)
            {
                if (!foodTables[i].isCorrect)
                {
                    foodTables[i].obj.GetComponent<Outline>().OutlineColor = Color.red;
                }
                if (!actionTables[i].isCorrect)
                {
                    actionTables[i].obj.GetComponent<Outline>().OutlineColor = Color.red;
                }
                missed = true;
                // spawnar prato errado
                if (foodTables[i].recipeID == -1)
                {

                    for (int j = 0; j < finalFoodPosition.Length; j++)
                    {
                        if (finalPlates[j] == null)
                        {
                            finalPlates[j] = Instantiate(errorFood, finalFoodPosition[j]);
                        }
                    }
                }
                else if (finalPlates[foodTables[i].recipeID] == null)
                {
                    finalPlates[foodTables[i].recipeID] =
                    Instantiate(errorFood, finalFoodPosition[foodTables[i].recipeID]);
                }
                //Debug.Log("ERROU!");
            }

            //spawnar prato certo e chama funções de vitória
            if (i + 1 == foodTables.Length)
            {
                for (int j = 0; j < foodTables.Length; j++)
                {
                    int recipeID = foodTables[j].recipeID;
                    //Debug.Log(recipeID);
                    if (recipeID != -1)
                    {
                        if (finalPlates[recipeID] == null)
                        {
                            finalPlates[recipeID] =
                            Instantiate(finalFood[recipeID],
                            finalFoodPosition[recipeID]);
                            //Debug.Log("ACERTOU!");
                        }
                    }
                }
                EndOfRead();
            }
        }
    }

    public void EndOfRead()
    {
        if (missed)
        {
            ResetAllFood();
            missed = false;
            hand.GetComponent<PickAndDrop>().enabled = true;
            ResumeGameplay();
        }
        else
        {
            startButton.SetActive(false);
            //Codigo para definir no GameManager que a fase foi concluida
            GameManager.Instance.UpdateProgress(SceneManager.GetActiveScene().name);
            StartCoroutine("Endgame");
        }
    }

    void ResetFinalFood()
    {
        for (int i = 0; i < finalPlates.Length; i++)
        {
            if (finalPlates[i] != null)
            {
                Destroy(finalPlates[i]);
                finalPlates[i] = null;
            }
        }
    }

    public void ResumeGameplay()
    {
        comecar = false;
        parar = true;
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerController>().ResetAnimation();
        cams[0].SetActive(true);
        cams[1].SetActive(false);
        GameManager.Instance.canPause = true;
    }
    //reinicia todos os insumos pro estado inicial
    public void ResetAllFood()
    {
        for (int i = 0; i < foodTables.Length; i++)
        {
            if (foodTables[i].obj != null)
            {
                ChangeState(foodTables[i].obj.GetComponent<Item>(), 0);
                GameObject smokeClone2 = Instantiate(smoke, foodTables[i].objPosition.transform.position, Quaternion.identity);
                Destroy(smokeClone2, 1);
            }
        }
    }

    IEnumerator Endgame()
    {
        cams[2].SetActive(true);
        cams[2].GetComponent<CinemachineVirtualCamera>().Priority = 11;
        jewelObj.GetComponent<Animator>().SetBool("abrir", true);
        yield return new WaitForSeconds(4);
        cams[2].GetComponent<CinemachineVirtualCamera>().Priority = 1;
        cams[2].SetActive(false);
        yield return new WaitForSeconds(2);
        winPanel.SetActive(true);
        yield return new WaitForSeconds(4);
        winPanel.SetActive(false);
        ResumeGameplay();
        dataManager.WinLevel();
    }
}
