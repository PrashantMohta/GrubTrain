
namespace GrubTrain
{   
    public class ModSettings 
    {
        public bool grubStrats {get; set;} = true;  // when enabled allows players to use grubs to pogo / collect soul using weaverlings
        public bool cursedStrats {get; set;} = false;  // when enabled makes grubs terrain 
        public int grubBaseCount {get; set;} = 3; 
        public int returnedCount {get; set;} = 0;
        public bool grubGathererMode {get; set;} = true; // each freed grub should add to number of grubs 
        public bool enableSounds {get; set;} = true; 
        public bool reduceSounds {get; set;} = true; // only play 30% of the sounds 
        public float moveSpeed {get; set;} = 13f;
        public float followDistance {get; set;} = 3f;

    }
}