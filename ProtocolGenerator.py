
import os
import random


def writeFirstLines (file_w, start_index, block_size, replay_failed_trial_type, arguments):
    file_w.write('StartIndex\t{}\n'.format(start_index))
    file_w.write('BlockSize\t{}\n'.format(block_size))
    file_w.write('ReplayFailedTrialType\t{}\n'.format(replay_failed_trial_type))

    file_w.write('{}\t{}\n'.format(arguments[0],arguments[1]))


def getScenesNames ():
    list = os.listdir("Assets/Resources/Scenes/VideoScenes")
    names = []
    for list_index in list:
        if os.path.splitext(list_index)[1] == '.unity':
            names.append(os.path.splitext(list_index)[0])
    return names


if __name__ == '__main__':

    wr = open('DefaultSubject/Protocol.txt', 'w', encoding='utf-8')

    protocol_values = [ 'Photo_ID' , 'Question_number' ]

    writeFirstLines(wr, 0, -1, 0, protocol_values)

    scene_names = getScenesNames()

    blocks = [ 1, 2, 3 ]
    size = 0

    for bloc_nb in blocks:

        for repeat in range(6):

            random.shuffle(scene_names)

            for scene in scene_names:
                wr.write('{}\t{}\n'.format (scene, bloc_nb))

