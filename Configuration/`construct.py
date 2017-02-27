f = open('cube.conf', 'w')
cnt = 0
for x in range(-2, 3, 2):
	for y in range(0, 5, 2):
		for z in range(4, 9, 2):
			f.write("create sphere\n")
			f.write("name sphere (%d)\n" % cnt)
			f.write("position %f %f %f\n" % (x, y, z))
			f.write("scale 0.5 0.5 0.5\n")
			f.write("end\n\n")
			cnt += 1
f.close()