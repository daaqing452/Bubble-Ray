import os

def getTime(s, e):
	arre = e.split(':')
	arrs = s.split(':')
	h = int(arre[0]) - int(arrs[0])
	m = int(arre[1]) - int(arrs[1])
	arre = arre[2].split('.')
	arrs = arrs[2].split('.')
	s = int(arre[0]) - int(arrs[0])
	q = int(arre[1]) - int(arrs[1])
	return h * 3600 + m * 60 + s + q * 0.000001

def parse(fname):
	if fname[-4:] != '.txt': return
	arr = fname.split('-')
	name = arr[0]
	subsection = arr[1]
	level = arr[2]
	technique = arr[3]
	f = open(name + '/' + fname, 'r')
	cntFalse = 0
	cntTrue = 0
	while True:
		line = f.readline()
		if len(line) == 0: break
		line = line[:-1]
		arr = line.split(' ')
		op = arr[0]
		if op == 'start':
			startTime = arr[1]
		elif op == 'select':
			obj = arr[1]
			torf = arr[2]#
			move = arr[3]
			endTime = arr[4]
			if torf == 'True':
				cntTrue += 1
			elif torf == 'False':
				cntFalse += 1
			else:
				pass
		else:
			pass
	f.close()
	fout.write(name + ',' + subsection + ',' + level[0] + ',' + level[1] + ',' + technique + ',' + str(getTime(startTime, endTime)) + ',' + move + ',' + str(1.0 * cntFalse / cntTrue) + '\n')	

# names = ['huyuan', 'guyizheng', 'luyiqin', 'yanyukang', 'xiexiaohui', 'liguohao']
names = ['zhanyulong']
fout = open(names[0] + '.csv', 'w')
fout.write('name,subsection,distance,size,technique,time,move,error\n')
for name in names:
	fnames = os.listdir(name + '/')
	for fname in fnames:
		parse(fname)
fout.close()