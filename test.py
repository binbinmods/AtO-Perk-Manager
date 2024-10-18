
class Hello():
    s = "hi"

x = Hello()

def f(test:Hello=None):
    if test == None:
        print("hello")
    else:
        print(test.s)


f()