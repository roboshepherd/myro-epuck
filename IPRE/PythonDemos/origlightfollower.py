from myro import *
#initialize("com4")

setIRPower(140)

print "Welcome, this program follows the light..."

# Enter an endless loop.
# In this loop, we will read some sensor values, decide what to do, then do it!
# The loop means we'll keep repeating this cycle over and over again, quickly.
# Known as "sense, think, act"
while(True):
    
    # Sense: Read the "bright" sensors (virtual light sensors)
    obstacle = getObstacle()
    obstacleLeft = obstacle[0]
    obstacleMiddle = obstacle[1]
    obstacleRight = obstacle[2]
    print "Obs:", obstacleLeft, ",", obstacleRight

    # Think: Perform a calculation on the sensor values to determine the
    # desired motion.
    leftMotorSpeed = 0.6 * (1.0 - (obstacleRight / 500))
    rightMotorSpeed = 0.6 * (1.0 - (obstacleLeft / 500))

    # For debugging, print the motor speeds
    print "Motors:", leftMotorSpeed, ",", rightMotorSpeed

    # Act: Set the motor speeds
    motors(leftMotorSpeed, rightMotorSpeed)
