using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerMesa : MonoBehaviour
{
    public Snap[] foodTables, actionTables;
    public GameObject[] finalFood;
    public GameObject errorFood;
    public GameObject[] finalFoods;
    public Transform[] finalFoodPosition;
    public Transform[] playerPositions;
    public GameObject[] cams;
    public bool comecar, parar;
    public GameObject player, hand;
    public ListaDeRecursos foodBank; // Acessa o banco de modelos de insumos
    public LeituraResultado LeituraFinal;
    private bool missed = false;
    public GameObject startButton;

    public void iniciar()
    {
        comecar = true;
        parar = false;
        if (comecar == true && parar == false)
        {
            StartCoroutine("TempoDeEspera");
            player.GetComponent<PlayerController>().enabled = false;
            hand.GetComponent<PickAndDrop>().enabled = false;
            cams[0].SetActive(false);
            cams[1].SetActive(true);
            ResetFinalFood();
        }

    }
    public IEnumerator TempoDeEspera()
    {
        for (int i = 0; i < foodTables.Length; i++)
        {
            if (!foodTables[i].isOccupied || !actionTables[i].isOccupied)
            {
                yield return new WaitForSeconds(1);
                missed = true;
                EndOfRead();
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
                        ChangeState(foodTables[i].obj.GetComponent<Item>(), 1);
                        break;
                    //Fritar
                    case "fry":
                        player.GetComponent<PlayerController>().animator.SetBool("Fritar", true);
                        player.GetComponent<PlayerController>().animator.SetBool("Cortar", false);
                        ChangeState(foodTables[i].obj.GetComponent<Item>(), 2);
                        break;
                }

                yield return new WaitForSeconds(2);
                //reseta a animação para parado
                player.GetComponent<PlayerController>().ResetAnimation();

                yield return new WaitForSeconds(0.5f);
                if (i + 1 == foodTables.Length)
                {
                    CheckFinal();
                }
            }
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
                // spawnar prato errado
                if (foodTables[i].obj.GetComponent<Item>().recipeID == -1)
                {
                    for (int j = 0; j < finalFoodPosition.Length; j++)
                    {
                        finalFoods[foodTables[i].obj.GetComponent<Item>().recipeID] = Instantiate(errorFood, finalFoodPosition[j]);
                    }
                }
                if (finalFoods[foodTables[i].obj.GetComponent<Item>().recipeID] == null)
                {
                    finalFoods[foodTables[i].obj.GetComponent<Item>().recipeID] =
                    Instantiate(errorFood, finalFoodPosition[foodTables[i].obj.GetComponent<Item>().recipeID]);
                }
                missed = true;
                //Debug.Log("ERROU!");
            }
            //spawnar prato certo e chama funções de vitória
            for (int j = 0; j < finalFood.Length; j++)
            {
                if (finalFoods[foodTables[j].obj.GetComponent<Item>().recipeID] == null)
                {
                    finalFoods[foodTables[i].obj.GetComponent<Item>().recipeID] =
                    Instantiate(finalFood[foodTables[i].obj.GetComponent<Item>().recipeID],
                    finalFoodPosition[foodTables[i].obj.GetComponent<Item>().recipeID]);
                    //Debug.Log("ACERTOU!");
                }
            }

            EndOfRead();
        }
    }

    public void EndOfRead()
    {
        if (missed)
        {
            ResetAllFood();
            missed = false;
            hand.GetComponent<PickAndDrop>().enabled = true;
        }
        else
        {
            startButton.SetActive(false);
        }

        comecar = false;
        parar = true;
        player.GetComponent<PlayerController>().enabled = true;
        player.GetComponent<PlayerController>().ResetAnimation();
        cams[0].SetActive(true);
        cams[1].SetActive(false);
    }

    void ResetFinalFood()
    {
        for (int i = 0; i < finalFoods.Length; i++)
        {
            if (finalFoods[i] != null)
            {
                Destroy(finalFoods[i]);
                finalFoods[i] = null;
            }
        }
    }
    //reinicia todos os insumos pro estado inicial
    public void ResetAllFood()
    {
        for (int i = 0; i < foodTables.Length; i++)
        {
            if (foodTables[i].obj != null)
            {
                ChangeState(foodTables[i].obj.GetComponent<Item>(), 0);
            }
        }
    }
}
