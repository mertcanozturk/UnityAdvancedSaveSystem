using UnityEngine;
public class Example : MonoBehaviour
{
    [System.Serializable]
    class TryJson
    {
        public string UserName;
        public string Password;
        public int Age;
    }

    void Start()
    {

        TryJson jsonClass = new TryJson()
        {
            UserName = "Mert",
            Password = "qwweerrttyy",
            Age = 15
        };


        Security.SaveJson<TryJson>(jsonClass, "json");

        TryJson received = Security.LoadJson<TryJson>("json");

        Debug.Log("UserName: " + received.UserName + " Password: " + received.Password + " Age: " + received.Age);

        Security.SaveBool("key1", true);
        bool key1 = Security.LoadBool("key1");

        Security.SaveFloat("key2", 0.1f);
        float key2 = Security.LoadFloat("key2");

        Security.SaveString("key3", "mert");
        string key3 = Security.LoadString("key3");

        Security.SaveInteger("key4", 1);
        int key4 = Security.LoadInteger("key4");
    }

}