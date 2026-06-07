using System.Collections;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public float GreenTime = 7f;
    public float YellowTime = 3f;
    public float RedTime = 10f;

    public GameObject GreenLight;
    public GameObject YellowLight;
    public GameObject RedLight;

    public GameObject PedestrianRedLight;
    public GameObject PedestrianGreenLight;

    [SerializeField] private StopScript Stopper;

    private void Start()
    {

        StartCoroutine(TrafficLightCycleRoutine());
    }

    private IEnumerator TrafficLightCycleRoutine()
    {
        while (true)
        {
            Stopper.CanGo(false);
            GreenLight.SetActive(true);
            YellowLight.SetActive(false);
            RedLight.SetActive(false);
            PedestrianRedLight.SetActive(true);
            PedestrianGreenLight.SetActive(false);

            yield return new WaitForSeconds(GreenTime);
            Stopper.CanGo(true);
            GreenLight.SetActive(false);
            YellowLight.SetActive(true);
            RedLight.SetActive(false);
            PedestrianRedLight.SetActive(true);
            PedestrianGreenLight.SetActive(false);

            yield return new WaitForSeconds(YellowTime);
            Stopper.CanGo(true);
            GreenLight.SetActive(false);
            YellowLight.SetActive(false);
            RedLight.SetActive(true);
            PedestrianRedLight.SetActive(false);
            PedestrianGreenLight.SetActive(true);

            yield return new WaitForSeconds(RedTime);
        }
    }
}