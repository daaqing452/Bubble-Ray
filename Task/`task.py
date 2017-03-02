import random

f = open('cubic5x5.conf', 'w')
f.write("trials 50\n\n")
cnt = 0
for x in range(-3, 6, 2):
	for y in range(-1, 8, 2):
		for z in range(4, 13, 2):
			f.write("object sphere\n")
			f.write("name sphere(%d)\n" % cnt)
			f.write("position %f %f %f\n" % (x, y, z))
			f.write("scale 0.3 0.3 0.3\n")
			f.write("end\n\n")
			cnt += 1
'''for i in range(8):
	x = random.random() * 10 - 5
	y = random.random() * 10
	z = random.random() * 5 + 8
	f.write("object sphere\n")
	f.write("name sphere(%d)\n" % cnt)
	f.write("position %f %f %f\n" % (x, y, z))
	f.write("scale 1 1 1\n")
	f.write("end\n\n")
	cnt += 1'''
f.close()